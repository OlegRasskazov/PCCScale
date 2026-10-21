#ifndef AD7794_H
#define AD7794_H

#include <stdint.h>

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

#define AD7794_DEFAULT_CONFIG                                                  \
  (AD7794_CFG_CH_AIN1 | AD7794_CFG_GAIN_128 | AD7794_CFG_BUFFER_ON |           \
   AD7794_CFG_BIPOLAR | AD7794_CFG_REF_EXT1)

#define AD7794_DEFAULT_MODE                                                    \
  (AD7794_MODE_CONTINUOUS | AD7794_MODE_CHOP_ON | AD7794_MODE_CLK_INTERNAL |   \
   AD7794_MODE_RATE_16_7HZ_60)

void AD7794_Reset(void);
uint8_t AD7794_ReadId(void);
void AD7794_WriteConfig(uint16_t config);
void AD7794_WriteMode(uint16_t mode);
uint32_t AD7794_ReadData(void);

#endif /* AD7794_H */
