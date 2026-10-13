static void AD7794_Reset(void) {
  uint8_t reset[4] = {0xFF, 0xFF, 0xFF, 0xFF};
  HAL_GPIO_WritePin(GPIOB, GPIO_PIN_9, GPIO_PIN_RESET); // CS LOW
  HAL_SPI_Transmit(&hspi2, reset, sizeof(reset), HAL_MAX_DELAY);
  HAL_GPIO_WritePin(GPIOB, GPIO_PIN_9, GPIO_PIN_SET); // CS HIGH
  HAL_Delay(1);
}

static uint8_t AD7794_ReadId(void) {
  uint8_t command = 0x60;
  uint8_t id = 0;
  HAL_GPIO_WritePin(GPIOB, GPIO_PIN_9, GPIO_PIN_RESET);
  HAL_SPI_Transmit(&hspi2, &command, 1, HAL_MAX_DELAY);
  HAL_SPI_Receive(&hspi2, &id, 1, HAL_MAX_DELAY);
  HAL_GPIO_WritePin(GPIOB, GPIO_PIN_9, GPIO_PIN_SET);
  return id;
}

static void AD7794_WriteConfig(uint16_t config) {
  uint8_t tx[3];
  tx[0] = 0x10; // Write Configuration Register
  tx[1] = (uint8_t)(config >> 8);
  tx[2] = (uint8_t)(config & 0xFF);

  HAL_GPIO_WritePin(GPIOB, GPIO_PIN_9, GPIO_PIN_RESET);
  HAL_SPI_Transmit(&hspi2, tx, sizeof(tx), HAL_MAX_DELAY);
  HAL_GPIO_WritePin(GPIOB, GPIO_PIN_9, GPIO_PIN_SET);
}
static void AD7794_WriteMode(uint16_t mode) {
  uint8_t tx[3];

  tx[0] = 0x08; // Write Mode Register
  tx[1] = (uint8_t)(mode >> 8);
  tx[2] = (uint8_t)(mode & 0xFF);

  HAL_GPIO_WritePin(GPIOB, GPIO_PIN_9, GPIO_PIN_RESET);
  HAL_SPI_Transmit(&hspi2, tx, sizeof(tx), HAL_MAX_DELAY);
  HAL_GPIO_WritePin(GPIOB, GPIO_PIN_9, GPIO_PIN_SET);
}
static uint32_t AD7794_ReadData(void) {
  uint8_t tx[4] = {0x58, 0xFF, 0xFF, 0xFF};
  uint8_t rx[4] = {0};

  HAL_GPIO_WritePin(GPIOB, GPIO_PIN_9, GPIO_PIN_RESET);
  HAL_SPI_TransmitReceive(&hspi2, tx, rx, sizeof(tx), HAL_MAX_DELAY);
  HAL_GPIO_WritePin(GPIOB, GPIO_PIN_9, GPIO_PIN_SET);

  return ((uint32_t)rx[1] << 16) | ((uint32_t)rx[2] << 8) | ((uint32_t)rx[3]);
}