#ifndef AD7794_H
#define AD7794_H

#include <stdint.h>
#include "stm32wbaxx_hal.h"
#include <stdbool.h>

// ============================================================
// AD7794 CONFIGURATION REGISTER (16-bit)
// Reset value: 0x0710
// ============================================================

// ------------------------------------------------------------
// CHANNEL SELECTION (bits 3:0)
// ------------------------------------------------------------
#define AD7794_CFG_CH_AIN1 0x0000
#define AD7794_CFG_CH_AIN2 0x0001
#define AD7794_CFG_CH_AIN3 0x0002
#define AD7794_CFG_CH_AIN4 0x0003
#define AD7794_CFG_CH_AIN5 0x0004
#define AD7794_CFG_CH_AIN6 0x0005
#define AD7794_CFG_CH_TEMP 0x0006
#define AD7794_CFG_CH_AVDD 0x0007
#define AD7794_CFG_CH_AIN1_SHORT 0x0008

#define AD7794_CFG_CH_MASK 0x000F

// ------------------------------------------------------------
// INPUT BUFFER (bit 4)
// ------------------------------------------------------------
#define AD7794_CFG_BUFFER_OFF 0x0000
#define AD7794_CFG_BUFFER_ON 0x0010

// ------------------------------------------------------------
// REFERENCE DETECTION (bit 5)
// ------------------------------------------------------------
#define AD7794_CFG_REF_DET_OFF 0x0000
#define AD7794_CFG_REF_DET_ON 0x0020

// ------------------------------------------------------------
// REFERENCE SOURCE (bits 7:6)
// ------------------------------------------------------------
#define AD7794_CFG_REF_EXT1 0x0000
#define AD7794_CFG_REF_EXT2 0x0040
#define AD7794_CFG_REF_INTERNAL 0x0080
// 0x00C0 is reserved

#define AD7794_CFG_REF_MASK 0x00C0

// ------------------------------------------------------------
// GAIN (bits 10:8)
// ------------------------------------------------------------
#define AD7794_CFG_GAIN_1 0x0000
#define AD7794_CFG_GAIN_2 0x0100
#define AD7794_CFG_GAIN_4 0x0200
#define AD7794_CFG_GAIN_8 0x0300
#define AD7794_CFG_GAIN_16 0x0400
#define AD7794_CFG_GAIN_32 0x0500
#define AD7794_CFG_GAIN_64 0x0600
#define AD7794_CFG_GAIN_128 0x0700

#define AD7794_CFG_GAIN_MASK 0x0700

// ------------------------------------------------------------
// BIAS GENERATOR BOOST (bit 11)
// ------------------------------------------------------------
#define AD7794_CFG_BOOST_OFF 0x0000
#define AD7794_CFG_BOOST_ON 0x0800

// ------------------------------------------------------------
// UNIPOLAR / BIPOLAR (bit 12)
// ------------------------------------------------------------
#define AD7794_CFG_BIPOLAR 0x0000
#define AD7794_CFG_UNIPOLAR 0x1000

// ------------------------------------------------------------
// BURNOUT CURRENTS (bit 13)
// ------------------------------------------------------------
#define AD7794_CFG_BURNOUT_OFF 0x0000
#define AD7794_CFG_BURNOUT_ON 0x2000

#define AD7794_BURNOUT_LOW 0x000100UL
#define AD7794_BURNOUT_HIGH 0xFFFEFFUL

// ------------------------------------------------------------
// BIAS VOLTAGE GENERATOR (bits 15:14)
// ------------------------------------------------------------
#define AD7794_CFG_BIAS_OFF 0x0000
#define AD7794_CFG_BIAS_AIN1_NEG 0x4000
#define AD7794_CFG_BIAS_AIN2_NEG 0x8000
#define AD7794_CFG_BIAS_AIN3_NEG 0xC000

#define AD7794_CFG_BIAS_MASK 0xC000

// ============================================================
// AD7794 MODE REGISTER (16-bit)
// Reset value: 0x000A
// ============================================================

// ------------------------------------------------------------
// FILTER UPDATE RATE (bits 3:0)
// Internal 64 kHz clock
// ------------------------------------------------------------
#define AD7794_MODE_RATE_470HZ 0x0001
#define AD7794_MODE_RATE_242HZ 0x0002
#define AD7794_MODE_RATE_123HZ 0x0003
#define AD7794_MODE_RATE_62HZ 0x0004
#define AD7794_MODE_RATE_50HZ 0x0005
#define AD7794_MODE_RATE_39HZ 0x0006
#define AD7794_MODE_RATE_33_2HZ 0x0007
#define AD7794_MODE_RATE_19_6HZ 0x0008
#define AD7794_MODE_RATE_16_7HZ_50 0x0009
#define AD7794_MODE_RATE_16_7HZ_60 0x000A
#define AD7794_MODE_RATE_12_5HZ 0x000B
#define AD7794_MODE_RATE_10HZ 0x000C
#define AD7794_MODE_RATE_8_33HZ 0x000D
#define AD7794_MODE_RATE_6_25HZ 0x000E
#define AD7794_MODE_RATE_4_17HZ 0x000F

// 0x0000 is reserved

#define AD7794_MODE_RATE_MASK 0x000F

// ------------------------------------------------------------
// CHOPPING (bit 4)
// ------------------------------------------------------------
#define AD7794_MODE_CHOP_ON 0x0000
#define AD7794_MODE_CHOP_OFF 0x0010

// ------------------------------------------------------------
// CLOCK SOURCE (bits 7:6)
// ------------------------------------------------------------
#define AD7794_MODE_CLK_INTERNAL 0x0000
#define AD7794_MODE_CLK_INTERNAL_OUT 0x0040
#define AD7794_MODE_CLK_EXTERNAL 0x0080
#define AD7794_MODE_CLK_EXTERNAL_DIV2 0x00C0

#define AD7794_MODE_CLK_MASK 0x00C0

// ------------------------------------------------------------
// IN-AMP COMMON MODE (bit 9)
// ------------------------------------------------------------
#define AD7794_MODE_AMP_CM_OFF 0x0000
#define AD7794_MODE_AMP_CM_ON 0x0200

// ------------------------------------------------------------
// POWER SWITCH (bit 12)
// ------------------------------------------------------------
#define AD7794_MODE_PSW_OFF 0x0000
#define AD7794_MODE_PSW_ON 0x1000

// ------------------------------------------------------------
// OPERATING MODE (bits 15:13)
// ------------------------------------------------------------
#define AD7794_MODE_CONTINUOUS 0x0000
#define AD7794_MODE_SINGLE 0x2000
#define AD7794_MODE_IDLE 0x4000
#define AD7794_MODE_POWER_DOWN 0x6000

#define AD7794_MODE_CAL_INT_ZERO 0x8000
#define AD7794_MODE_CAL_INT_FULL 0xA000
#define AD7794_MODE_CAL_SYS_ZERO 0xC000
#define AD7794_MODE_CAL_SYS_FULL 0xE000

#define AD7794_MODE_OP_MASK 0xE000

// ============================================================
// DEFAULT PCCScale CONFIGURATION
// ============================================================

#define AD7794_MAX_DEVICES 10U
#define AD7794_CHANNEL_COUNT 6U

#define AD7794_STATUS_RDY 0x80U
#define AD7794_STATUS_ERR 0x40U
#define AD7794_STATUS_NOXREF 0x20U
#define AD7794_STATUS_CH_MASK 0x07U

#define AD7794_TIMEOUT_MS 1000U

#define AD7794_DEFAULT_CONFIG                                                  \
  (AD7794_CFG_CH_AIN1 | AD7794_CFG_GAIN_128 | AD7794_CFG_BUFFER_ON |           \
   AD7794_CFG_BIPOLAR | AD7794_CFG_REF_EXT1)

#define AD7794_DEFAULT_MODE                                                    \
  (AD7794_MODE_CONTINUOUS | AD7794_MODE_CHOP_ON | AD7794_MODE_CLK_INTERNAL |   \
   AD7794_MODE_RATE_16_7HZ_60)

typedef enum { 
  AD7794_PHASE_BURNOUT,
  AD7794_PHASE_NORMAL 
} AD7794_Phase;

typedef struct {
  GPIO_TypeDef *port;
  uint16_t pin;
} AD7794_ChipSelect;

typedef struct {
  AD7794_ChipSelect cs;

  uint8_t currentChannel;
  uint8_t id;

  AD7794_Phase phase;

  uint8_t diagnosticDoneMask;
  uint8_t suspectedOpenMask;
  uint8_t channelFaultMask;

  uint8_t burnoutStatus[AD7794_CHANNEL_COUNT];
  uint8_t normalStatus[AD7794_CHANNEL_COUNT];
  uint32_t burnoutRaw[AD7794_CHANNEL_COUNT];
  
  uint32_t raw[AD7794_CHANNEL_COUNT];
  uint32_t updatedAt[AD7794_CHANNEL_COUNT];

  uint8_t validMask;
  uint8_t errorCount;

  uint32_t channelStartedAt;

  bool initialized;
  bool fault;

} AD7794_Device;

// Initialize all ADCs
void AD7794_Init(SPI_HandleTypeDef *spi, const AD7794_ChipSelect *chipSelects,
                 uint8_t count);

// Nonblocking conversion polling
void AD7794_Process(void);

// Get latest measurement
bool AD7794_GetRaw(uint8_t device, uint8_t channel, uint32_t *value);

// Access device state
const AD7794_Device *AD7794_GetDevice(uint8_t device);

// Obsolete
// void AD7794_Reset(void);
// uint8_t AD7794_ReadId(void);
// void AD7794_WriteConfig(uint16_t config);
// void AD7794_WriteMode(uint16_t mode);
// uint32_t AD7794_ReadData(void);

#endif /* AD7794_H */