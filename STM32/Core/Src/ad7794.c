#include "ad7794.h"
#include "gpio.h"
#include "spi.h"
#include <string.h>

#define AD7794_REG_STATUS_READ 0x40U
#define AD7794_REG_ID_READ 0x60U
#define AD7794_REG_DATA_READ 0x58U
#define AD7794_REG_CONFIG_WRITE 0x10U
#define AD7794_REG_MODE_WRITE 0x08U

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

static bool SetChannel(AD7794_Device *dev, uint8_t channel) {
  if (channel >= AD7794_CHANNEL_COUNT)
    return false;

  uint16_t config = AD7794_CFG_GAIN_128 | AD7794_CFG_BUFFER_ON | channel;

  if (!WriteRegister16(dev, AD7794_REG_CONFIG_WRITE, config))
    return false;

  dev->currentChannel = channel;
  dev->channelStartedAt = HAL_GetTick();

  return true;
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

    if (!SetChannel(dev, 0)) {
      dev->fault = true;
      continue;
    }

    uint16_t mode = AD7794_MODE_CONTINUOUS | AD7794_MODE_RATE_19_6HZ;

    if (!WriteRegister16(dev, AD7794_REG_MODE_WRITE, mode)) {
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

    uint32_t elapsed = now - dev->channelStartedAt;

    // Conversion timeout
    // if (elapsed > AD7794_TIMEOUT_MS) {
    //   dev->fault = true;
    //   continue;
    // }

    uint8_t status;

    if (!ReadStatus(dev, &status)) {
      dev->fault = true;
      continue;
    }

    // ADC is still converting
    if (status & AD7794_STATUS_RDY)
      continue;

    // ADC reported a conversion/reference error
    if (status & (AD7794_STATUS_ERR | AD7794_STATUS_NOXREF)) {
      dev->fault = true;
      continue;
    }

    uint8_t channel = dev->currentChannel;

    // Check that the result belongs
    // to the channel currently selected.
    if ((status & AD7794_STATUS_CH_MASK) != channel) {
      dev->fault = true;
      continue;
    }

    uint32_t raw;

    if (!ReadData(dev, &raw)) {
      dev->fault = true;
      continue;
    }

    // Store the measurement
    dev->raw[channel] = raw;
    dev->updatedAt[channel] = HAL_GetTick();
    dev->validMask |= (uint8_t)(1U << channel);

    // Move to the next AIN channel
    uint8_t nextChannel = (uint8_t)((channel + 1U) % AD7794_CHANNEL_COUNT);

    if (!SetChannel(dev, nextChannel)) {
      dev->fault = true;
      continue;
    }
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