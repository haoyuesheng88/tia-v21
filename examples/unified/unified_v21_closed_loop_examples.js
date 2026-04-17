"use strict";

/*
WinCC Unified V21 JavaScript examples for a simple PLC/HMI closed loop.

Recommended Unified tags:
- HMI_Start_PB   -> %M10.0
- HMI_Stop_PB    -> %M10.1
- HMI_Run_Cmd    -> %M10.2
- HMI_Lamp_FB    -> %M10.3
- HMI_SetValue   -> %MW20
- HMI_ActualValue-> %MW22

Place these snippets into Unified screen/button/task events.
*/

export function Button_Start_Press() {
  Tags("HMI_Start_PB").Write(
    true,
    HMIRuntime.Tags.Enums.hmiWriteType.hmiWriteWait
  );

  HMIRuntime.Timers.SetTimeout(function ResetStartPulse() {
    Tags("HMI_Start_PB").Write(
      false,
      HMIRuntime.Tags.Enums.hmiWriteType.hmiWriteWait
    );
  }, 200);
}

export function Button_Stop_Press() {
  Tags("HMI_Stop_PB").Write(
    true,
    HMIRuntime.Tags.Enums.hmiWriteType.hmiWriteWait
  );

  HMIRuntime.Timers.SetTimeout(function ResetStopPulse() {
    Tags("HMI_Stop_PB").Write(
      false,
      HMIRuntime.Tags.Enums.hmiWriteType.hmiWriteWait
    );
  }, 200);
}

export function IOField_SetValue_Change(newValue) {
  Tags("HMI_SetValue").Write(
    Number(newValue),
    HMIRuntime.Tags.Enums.hmiWriteType.hmiWriteWait
  );
}

export function Task_ReadFeedback_Update() {
  let ts = Tags.CreateTagSet([
    "HMI_Run_Cmd",
    "HMI_Lamp_FB",
    "HMI_SetValue",
    "HMI_ActualValue",
  ]);

  ts.Read(HMIRuntime.Tags.Enums.hmiReadType.hmiReadDirect);

  HMIRuntime.Trace(
    "Run=" +
      ts("HMI_Run_Cmd").Value +
      ", Lamp=" +
      ts("HMI_Lamp_FB").Value +
      ", Set=" +
      ts("HMI_SetValue").Value +
      ", Actual=" +
      ts("HMI_ActualValue").Value
  );
}

export function Button_StartAndPreset_Press() {
  let ts = Tags.CreateTagSet([
    ["HMI_SetValue", 1200],
    ["HMI_Start_PB", true],
  ]);

  ts.Write(HMIRuntime.Tags.Enums.hmiWriteType.hmiWriteWait);

  HMIRuntime.Timers.SetTimeout(function ResetPulse() {
    Tags("HMI_Start_PB").Write(
      false,
      HMIRuntime.Tags.Enums.hmiWriteType.hmiWriteWait
    );
  }, 200);
}

/*
Suggested screen bindings:
- Start button -> event Press -> Button_Start_Press
- Stop button -> event Press -> Button_Stop_Press
- Set value IO field -> value change -> IOField_SetValue_Change
- Cyclic task or screen event -> Task_ReadFeedback_Update
- Indicator lamp -> bind color/visibility to HMI_Lamp_FB
- Numeric display -> bind to HMI_ActualValue
*/
