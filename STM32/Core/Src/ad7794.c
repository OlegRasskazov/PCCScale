#include "ad7794.h"
#include "gpio.h"
#include "spi.h"
#include <stdint.h>
#include <string.h>

#define AD7794_REG_STATUS_READ 0x40U
#define AD7794_REG_ID_READ 0x60U
#define AD7794_REG_DATA_READ 0x58U
#define AD7794_REG_CONFIG_WRITE 0x10U
#define AD7794_REG_MODE_WRITE 0x08U

#define AD7794_ALL_CHANNELS_MASK 0x3FU

static SPI_HandleTypeDef *adcSpi;

static AD7794_Device devices[AD7794_MAX_DEVICES];
static uint8_t deviceCount = 0;

// --------------------------------------------------
// SPI helpers
// --------------------------------------------------

static void Select(const AD7794_Device *dev) {
  HAL_GPIO_WritePin(dev->cs.port, dev->cs.pin, GPIO_PIN_RESET);
}

static void Deselect(const AD7794_Device *dev) {
  HAL_GPIO_WritePin(dev->cs.port, dev->cs.pin, GPIO_PIN_SET);
}

static bool Transfer(AD7794_Device *dev, const uint8_t *tx, uint8_t *rx,
                     uint16_t size) {
  Select(dev);

  HAL_StatusTypeDef result =
      HAL_SPI_TransmitReceive(adcSpi, (uint8_t *)tx, rx, size, 20U);

  Deselect(dev);

  return result == HAL_OK;
}

// --------------------------------------------------
// ADC register access
// --------------------------------------------------

static bool Reset(AD7794_Device *dev) {
  uint8_t tx[4] = {0xFF, 0xFF, 0xFF, 0xFF};
  uint8_t rx[4] = {0};

  return Transfer(dev, tx, rx, 4);
}

static bool ReadId(AD7794_Device *dev, uint8_t *id) {
  uint8_t tx[2] = {AD7794_REG_ID_READ, 0xFF};
  uint8_t rx[2] = {0};

  if (!Transfer(dev, tx, rx, 2))
    return false;

  *id = rx[1];
  return true;
}

static bool ReadStatus(AD7794_Device *dev, uint8_t *status) {
  uint8_t tx[2] = {AD7794_REG_STATUS_READ, 0xFF};
  uint8_t rx[2] = {0};

  if (!Transfer(dev, tx, rx, 2))
    return false;

  *status = rx[1];
  return true;
}

static bool ReadData(AD7794_Device *dev, uint32_t *value) {
  uint8_t tx[4] = {AD7794_REG_DATA_READ, 0xFF, 0xFF, 0xFF};

  uint8_t rx[4] = {0};

  if (!Transfer(dev, tx, rx, 4))
    return false;

  *value = ((uint32_t)rx[1] << 16) | ((uint32_t)rx[2] << 8) | ((uint32_t)rx[3]);

  return true;
}

static bool WriteRegister16(AD7794_Device *dev, uint8_t command,
                            uint16_t value) {
  uint8_t tx[3] = {command, (uint8_t)(value >> 8), (uint8_t)value};

  uint8_t rx[3] = {0};

  return Transfer(dev, tx, rx, 3);
}

// --------------------------------------------------
// Channel selection
// --------------------------------------------------

static bool SetChannel(AD7794_Device *dev, uint8_t channel, bool burnout) {
  if (channel >= AD7794_CHANNEL_COUNT)
    return false;

  uint16_t config = AD7794_CFG_GAIN_128 | AD7794_CFG_BUFFER_ON | channel;

  if (burnout)
    config |= AD7794_CFG_BURNOUT_ON;

  if (!WriteRegister16(dev, AD7794_REG_CONFIG_WRITE, config))
    return false;

  dev->currentChannel = channel;
  dev->channelStartedAt = HAL_GetTick();

  return true;
}

static int8_t GetNextActiveChannel(const AD7794_Device *dev,
                                   uint8_t currentChannel) {
  uint8_t activeMask =
      dev->diagnosticDoneMask & (uint8_t)~dev->suspectedOpenMask;

  if (activeMask == 0)
    return -1;

  for (uint8_t offset = 1; offset <= AD7794_CHANNEL_COUNT; offset++) {
    uint8_t channel = (currentChannel + offset) % AD7794_CHANNEL_COUNT;

    if (activeMask & (1U << channel))
      return (int8_t)channel;
  }

  return -1;
}

static int8_t GetNextChannel(const AD7794_Device *dev, uint8_t currentChannel) {
  // Initial diagnostics not completed yet
  if (dev->diagnosticDoneMask != AD7794_ALL_CHANNELS_MASK) {
    return (int8_t)((currentChannel + 1U) % AD7794_CHANNEL_COUNT);
  }

  return GetNextActiveChannel(dev, currentChannel);
}

static bool AdvanceChannel(AD7794_Device *dev) {
  int8_t next = GetNextChannel(dev, dev->currentChannel);

  if (next < 0) {
    // No connected load cells detected.
    // This is not a hardware ADC fault.
    dev->initialized = false;
    return true;
  }

  uint8_t nextChannel = (uint8_t)next;

  bool needsDiagnostic = !(dev->diagnosticDoneMask & (1U << nextChannel));

  dev->phase = needsDiagnostic ? AD7794_PHASE_BURNOUT : AD7794_PHASE_NORMAL;

  return SetChannel(dev, nextChannel, needsDiagnostic);
}

// --------------------------------------------------
// Initialization
// --------------------------------------------------

void AD7794_Init(SPI_HandleTypeDef *spi, const AD7794_ChipSelect *chipSelects,
                 uint8_t count) {
  adcSpi = spi;
  deviceCount = 0;

  memset(devices, 0, sizeof(devices));

  if (spi == NULL || chipSelects == NULL || count > AD7794_MAX_DEVICES)
    return;

  deviceCount = count;

  // Ensure no ADC is selected
  for (uint8_t i = 0; i < count; i++) {
    devices[i].cs = chipSelects[i];

    Deselect(&devices[i]);
  }

  for (uint8_t i = 0; i < count; i++) {
    AD7794_Device *dev = &devices[i];

    if (!Reset(dev)) {
      dev->fault = true;
      continue;
    }

    // Initialization-only wait after reset
    HAL_Delay(1);

    if (!ReadId(dev, &dev->id)) {
      dev->fault = true;
      continue;
    }

    // Prototype AD7794 identification
    if ((dev->id & 0xF0U) != 0x40U) {
      dev->fault = true;
      continue;
    }

    uint16_t mode = AD7794_MODE_CONTINUOUS | AD7794_MODE_RATE_19_6HZ;

    if (!WriteRegister16(dev, AD7794_REG_MODE_WRITE, mode)) {
      dev->fault = true;
      continue;
    }

    dev->phase = AD7794_PHASE_BURNOUT;
    dev->diagnosticDoneMask = 0;
    dev->suspectedOpenMask = 0;

    if (!SetChannel(dev, 0, true)) {
      dev->fault = true;
      continue;
    }

    dev->channelStartedAt = HAL_GetTick();
    dev->initialized = true;
  }
}

// --------------------------------------------------
// Periodic ADC processing
// --------------------------------------------------

void AD7794_Process(void) {
  uint32_t now = HAL_GetTick();

  for (uint8_t i = 0; i < deviceCount; i++) {
    AD7794_Device *dev = &devices[i];

    if (!dev->initialized || dev->fault)
      continue;

    uint8_t channel = dev->currentChannel;
    uint8_t mask = (uint8_t)(1U << channel);

    // -----------------------------------------
    // Conversion timeout
    // -----------------------------------------

    uint32_t elapsed = now - dev->channelStartedAt;

    // if (elapsed > AD7794_TIMEOUT_MS) {
    //   dev->fault = true;
    //   continue;
    // }

    // -----------------------------------------
    // Read ADC status
    // -----------------------------------------

    uint8_t status;

    if (!ReadStatus(dev, &status)) {
      dev->fault = true;
      continue;
    }

    // ADC is still converting
    if (status & AD7794_STATUS_RDY)
      continue;

    // Reference voltage error
    if (status & AD7794_STATUS_NOXREF) {
      dev->fault = true;
      continue;
    }

    // Verify channel number
    if ((status & AD7794_STATUS_CH_MASK) != channel) {
      dev->fault = true;
      continue;
    }

    // -----------------------------------------
    // Read ADC data
    // -----------------------------------------

    uint32_t raw;

    if (!ReadData(dev, &raw)) {
      dev->fault = true;
      continue;
    }

    // -----------------------------------------
    // BURNOUT DIAGNOSTICS
    // -----------------------------------------

    if (dev->phase == AD7794_PHASE_BURNOUT) {
      dev->burnoutRaw[channel] = raw;
      dev->burnoutStatus[channel] = status;

      dev->diagnosticDoneMask |= mask;

      // Saturation indicates possible open circuit
      if ((status & AD7794_STATUS_ERR) || raw <= AD7794_BURNOUT_LOW ||
          raw >= AD7794_BURNOUT_HIGH) {
        dev->suspectedOpenMask |= mask;
      } else {
        dev->suspectedOpenMask &= (uint8_t)~mask;
      }

      // Disable burnout and measure normally
      dev->phase = AD7794_PHASE_NORMAL;

      if (!SetChannel(dev, channel, false))
        dev->fault = true;

      continue;
    }

    // -----------------------------------------
    // NORMAL MEASUREMENT
    // -----------------------------------------

    dev->normalStatus[channel] = status;

    if (status & AD7794_STATUS_ERR) {
      // Error affects only this AIN channel
      dev->channelFaultMask |= mask;

      // Invalidate previous measurement
      dev->validMask &= (uint8_t)~mask;

      // Continue with another channel
      if (!AdvanceChannel(dev))
        dev->fault = true;

      continue;
    }

    // Successful measurement
    dev->channelFaultMask &= (uint8_t)~mask;

    dev->raw[channel] = raw;
    dev->updatedAt[channel] = HAL_GetTick();
    dev->validMask |= mask;

    // -----------------------------------------
    // SELECT NEXT CHANNEL
    // -----------------------------------------

    if (!AdvanceChannel(dev))
      dev->fault = true;
  }
}

// --------------------------------------------------
// Public accessors
// --------------------------------------------------

bool AD7794_GetRaw(uint8_t device, uint8_t channel, uint32_t *value) {
  if (device >= deviceCount || channel >= AD7794_CHANNEL_COUNT || value == NULL)
    return false;

  AD7794_Device *dev = &devices[device];

  if (dev->fault || !(dev->validMask & (1U << channel)))
    return false;

  *value = dev->raw[channel];

  return true;
}

const AD7794_Device *AD7794_GetDevice(uint8_t device) {
  if (device >= deviceCount)
    return NULL;

  return &devices[device];
}