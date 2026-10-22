#include "ble_notify.h"
#include "ad7794.h"
#include "lcs1.h"

#define ADC_COUNT 10U
#define CHANNEL_COUNT 6U

static uint32_t lastSentAt[ADC_COUNT][CHANNEL_COUNT];

static uint8_t nextAdc = 0;
static uint8_t nextChannel = 0;

void BLE_Notify_Init(void) {
  for (uint8_t a = 0; a < ADC_COUNT; a++) {
    for (uint8_t c = 0; c < CHANNEL_COUNT; c++) {
      lastSentAt[a][c] = 0;
    }
  }
}

void BLE_Notify_Process(void) {
  for (uint8_t i = 0; i < ADC_COUNT * CHANNEL_COUNT; i++) {
    uint8_t adc = nextAdc;
    uint8_t channel = nextChannel;

    // Advance cursor
    nextChannel++;

    if (nextChannel >= CHANNEL_COUNT) {
      nextChannel = 0;
      nextAdc = (nextAdc + 1) % ADC_COUNT;
    }

    const AD7794_Device *dev = AD7794_GetDevice(adc);

    if (dev == NULL || dev->fault)
      continue;

    if (!(dev->validMask & (1U << channel)))
      continue;

    uint32_t timestamp = dev->updatedAt[channel];

    if (timestamp == 0 || timestamp == lastSentAt[adc][channel])
      continue;

    uint32_t raw = dev->raw[channel];

    uint8_t packet[6];

    // Oven ID, Pan ID
    packet[0] = adc + 1;
    packet[1] = channel + 1;

    // ADC raw value (Little Endian)
    packet[2] = (uint8_t)(raw);
    packet[3] = (uint8_t)(raw >> 8);
    packet[4] = (uint8_t)(raw >> 16);
    packet[5] = (uint8_t)(raw >> 24);

    LCS1_Data_t weightData = {.p_Payload = packet, .Length = sizeof(packet)};

    tBleStatus result = LCS1_UpdateValue(LCS1_WEIGHT, &weightData);

    if (result == BLE_STATUS_SUCCESS) {
      lastSentAt[adc][channel] = timestamp;
    }

    // Send at most one notification per invocation
    return;
  }
}