/* USER CODE BEGIN Header */
/**
  ******************************************************************************
  * @file    LCS1_app.c
  * @author  MCD Application Team
  * @brief   LCS1_app application definition.
  ******************************************************************************
  * @attention
  *
  * Copyright (c) 2026 STMicroelectronics.
  * All rights reserved.
  *
  * This software is licensed under terms that can be found in the LICENSE file
  * in the root directory of this software component.
  * If no LICENSE file comes with this software, it is provided AS-IS.
  *
  ******************************************************************************
  */
/* USER CODE END Header */

/* Includes ------------------------------------------------------------------*/
#include "main.h"
#include "app_common.h"
#include "log_module.h"
#include "app_ble.h"
#include "ll_sys_if.h"
#include "dbg_trace.h"
#include "lcs1_app.h"
#include "lcs1.h"
#include "stm32_rtos.h"

/* Private includes ----------------------------------------------------------*/
/* USER CODE BEGIN Includes */

/* USER CODE END Includes */

/* Private typedef -----------------------------------------------------------*/
/* USER CODE BEGIN PTD */
typedef enum {
  CMD_SET_THRESHOLD = 0x01
} Command_t;
/* USER CODE END PTD */

typedef enum
{
  Weight_NOTIFICATION_OFF,
  Weight_NOTIFICATION_ON,
  Status_NOTIFICATION_OFF,
  Status_NOTIFICATION_ON,
  /* USER CODE BEGIN Service1_APP_SendInformation_t */

  /* USER CODE END Service1_APP_SendInformation_t */
  LCS1_APP_SENDINFORMATION_LAST
} LCS1_APP_SendInformation_t;

typedef struct
{
  LCS1_APP_SendInformation_t     Weight_Notification_Status;
  LCS1_APP_SendInformation_t     Status_Notification_Status;
  /* USER CODE BEGIN Service1_APP_Context_t */

  /* USER CODE END Service1_APP_Context_t */
  uint16_t              ConnectionHandle;
} LCS1_APP_Context_t;

/* Private defines -----------------------------------------------------------*/
/* USER CODE BEGIN PD */
static int32_t thresholds[8][6];
/* USER CODE END PD */

/* External variables --------------------------------------------------------*/
/* USER CODE BEGIN EV */

/* USER CODE END EV */

/* Private macros ------------------------------------------------------------*/
/* USER CODE BEGIN PM */

/* USER CODE END PM */

/* Private variables ---------------------------------------------------------*/
static LCS1_APP_Context_t LCS1_APP_Context;

uint8_t a_LCS1_UpdateCharData[247];

/* USER CODE BEGIN PV */

/* USER CODE END PV */

/* Private function prototypes -----------------------------------------------*/
static void LCS1_Weight_SendNotification(void);
static void LCS1_Status_SendNotification(void);

/* USER CODE BEGIN PFP */

/* USER CODE END PFP */

/* Functions Definition ------------------------------------------------------*/
void LCS1_Notification(LCS1_NotificationEvt_t *p_Notification)
{
  /* USER CODE BEGIN Service1_Notification_1 */

  /* USER CODE END Service1_Notification_1 */
  switch(p_Notification->EvtOpcode)
  {
    /* USER CODE BEGIN Service1_Notification_Service1_EvtOpcode */

    /* USER CODE END Service1_Notification_Service1_EvtOpcode */

    case LCS1_WEIGHT_NOTIFY_ENABLED_EVT:
      /* USER CODE BEGIN Service1Char1_NOTIFY_ENABLED_EVT */

      /* USER CODE END Service1Char1_NOTIFY_ENABLED_EVT */
      break;

    case LCS1_WEIGHT_NOTIFY_DISABLED_EVT:
      /* USER CODE BEGIN Service1Char1_NOTIFY_DISABLED_EVT */

      /* USER CODE END Service1Char1_NOTIFY_DISABLED_EVT */
      break;

    case LCS1_COMMAND_WRITE_EVT:
      /* USER CODE BEGIN Service1Char2_WRITE_EVT */

      uint8_t *data = p_Notification->DataTransfered.p_Payload;
      uint16_t length = p_Notification->DataTransfered.Length;

      if (length < 1)
        break;

      Command_t command = (Command_t)data[0];

      switch (command) {
      case CMD_SET_THRESHOLD: {
        if (length != 7)
          break;

        uint8_t rack = data[1];
        uint8_t pan = data[2];

        if (rack >= 8 || pan >= 6)
          break;

        int32_t threshold =
            (int32_t)(((uint32_t)data[3]) | ((uint32_t)data[4] << 8) |
                      ((uint32_t)data[5] << 16) | ((uint32_t)data[6] << 24));

        thresholds[rack][pan] = threshold;

        break;
      }

      default:
        break;
      }

      break;

      /* USER CODE END Service1Char2_WRITE_EVT */
      break;

    case LCS1_STATUS_READ_EVT:
      /* USER CODE BEGIN Service1Char3_READ_EVT */

      /* USER CODE END Service1Char3_READ_EVT */
      break;

    case LCS1_STATUS_NOTIFY_ENABLED_EVT:
      /* USER CODE BEGIN Service1Char3_NOTIFY_ENABLED_EVT */

      /* USER CODE END Service1Char3_NOTIFY_ENABLED_EVT */
      break;

    case LCS1_STATUS_NOTIFY_DISABLED_EVT:
      /* USER CODE BEGIN Service1Char3_NOTIFY_DISABLED_EVT */

      /* USER CODE END Service1Char3_NOTIFY_DISABLED_EVT */
      break;

    default:
      /* USER CODE BEGIN Service1_Notification_default */

      /* USER CODE END Service1_Notification_default */
      break;
  }
  /* USER CODE BEGIN Service1_Notification_2 */

  /* USER CODE END Service1_Notification_2 */
  return;
}

void LCS1_APP_EvtRx(LCS1_APP_ConnHandleNotEvt_t *p_Notification)
{
  /* USER CODE BEGIN Service1_APP_EvtRx_1 */

  /* USER CODE END Service1_APP_EvtRx_1 */

  switch(p_Notification->EvtOpcode)
  {
    /* USER CODE BEGIN Service1_APP_EvtRx_Service1_EvtOpcode */

    /* USER CODE END Service1_APP_EvtRx_Service1_EvtOpcode */
    case LCS1_CONN_HANDLE_EVT :
      /* USER CODE BEGIN Service1_APP_CONN_HANDLE_EVT */

      /* USER CODE END Service1_APP_CONN_HANDLE_EVT */
      break;

    case LCS1_DISCON_HANDLE_EVT :
      /* USER CODE BEGIN Service1_APP_DISCON_HANDLE_EVT */

      /* USER CODE END Service1_APP_DISCON_HANDLE_EVT */
      break;

    default:
      /* USER CODE BEGIN Service1_APP_EvtRx_default */

      /* USER CODE END Service1_APP_EvtRx_default */
      break;
  }

  /* USER CODE BEGIN Service1_APP_EvtRx_2 */

  /* USER CODE END Service1_APP_EvtRx_2 */

  return;
}

void LCS1_APP_Init(void)
{
  UNUSED(LCS1_APP_Context);
  LCS1_Init();

  /* USER CODE BEGIN Service1_APP_Init */

  /* USER CODE END Service1_APP_Init */
  return;
}

/* USER CODE BEGIN FD */

/* USER CODE END FD */

/*************************************************************
 *
 * LOCAL FUNCTIONS
 *
 *************************************************************/
__USED void LCS1_Weight_SendNotification(void) /* Property Notification */
{
  LCS1_APP_SendInformation_t notification_on_off = Weight_NOTIFICATION_OFF;
  LCS1_Data_t lcs1_notification_data;

  lcs1_notification_data.p_Payload = (uint8_t*)a_LCS1_UpdateCharData;
  lcs1_notification_data.Length = 0;

  /* USER CODE BEGIN Service1Char1_NS_1 */

  /* USER CODE END Service1Char1_NS_1 */

  if (notification_on_off != Weight_NOTIFICATION_OFF)
  {
    LCS1_UpdateValue(LCS1_WEIGHT, &lcs1_notification_data);
  }

  /* USER CODE BEGIN Service1Char1_NS_Last */

  /* USER CODE END Service1Char1_NS_Last */

  return;
}

__USED void LCS1_Status_SendNotification(void) /* Property Notification */
{
  LCS1_APP_SendInformation_t notification_on_off = Status_NOTIFICATION_OFF;
  LCS1_Data_t lcs1_notification_data;

  lcs1_notification_data.p_Payload = (uint8_t*)a_LCS1_UpdateCharData;
  lcs1_notification_data.Length = 0;

  /* USER CODE BEGIN Service1Char3_NS_1 */

  /* USER CODE END Service1Char3_NS_1 */

  if (notification_on_off != Status_NOTIFICATION_OFF)
  {
    LCS1_UpdateValue(LCS1_STATUS, &lcs1_notification_data);
  }

  /* USER CODE BEGIN Service1Char3_NS_Last */

  /* USER CODE END Service1Char3_NS_Last */

  return;
}

/* USER CODE BEGIN FD_LOCAL_FUNCTIONS */

/* USER CODE END FD_LOCAL_FUNCTIONS */
