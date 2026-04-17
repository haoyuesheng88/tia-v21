"use strict";

const assert = require("node:assert/strict");

function createMockRuntime(initialState = {}) {
  const state = Object.assign(
    {
      HMI_Start_PB: false,
      HMI_Stop_PB: false,
      HMI_Run_Cmd: false,
      HMI_Lamp_FB: false,
      HMI_SetValue: 0,
      HMI_ActualValue: 0,
      Q0_0: false,
    },
    initialState
  );

  const traces = [];
  const timeouts = [];

  function plcScan() {
    if (state.HMI_Start_PB) {
      state.HMI_Run_Cmd = true;
      state.HMI_Start_PB = false;
    }

    if (state.HMI_Stop_PB) {
      state.HMI_Run_Cmd = false;
      state.HMI_Stop_PB = false;
    }

    state.Q0_0 = state.HMI_Run_Cmd;
    state.HMI_Lamp_FB = state.Q0_0;
    state.HMI_ActualValue = state.HMI_SetValue;
  }

  function Tags(name) {
    return {
      get Value() {
        return state[name];
      },
      set Value(v) {
        state[name] = v;
      },
      Read() {
        return state[name];
      },
      Write(v) {
        if (typeof v !== "undefined") {
          state[name] = v;
        }
        return 0;
      },
    };
  }

  Tags.CreateTagSet = function CreateTagSet(items) {
    const map = new Map();

    for (const item of items) {
      if (Array.isArray(item)) {
        map.set(item[0], { Value: item[1], LastError: 0, ErrorDescription: "" });
      } else {
        map.set(item, { Value: state[item], LastError: 0, ErrorDescription: "" });
      }
    }

    function accessor(tagName) {
      return map.get(tagName);
    }

    accessor.Read = function Read() {
      for (const [name, tag] of map.entries()) {
        tag.Value = state[name];
      }
    };

    accessor.Write = function Write() {
      for (const [name, tag] of map.entries()) {
        state[name] = tag.Value;
      }
    };

    return accessor;
  };

  const HMIRuntime = {
    Tags: {
      Enums: {
        hmiWriteType: {
          hmiWriteNoWait: 0,
          hmiWriteWait: 1,
        },
        hmiReadType: {
          hmiReadCache: 0,
          hmiReadDirect: 1,
        },
      },
    },
    Timers: {
      SetTimeout(callback, delay) {
        timeouts.push({ callback, delay });
        return timeouts.length;
      },
      runAll() {
        while (timeouts.length) {
          const timer = timeouts.shift();
          timer.callback();
        }
      },
    },
    Trace(message) {
      traces.push(String(message));
    },
  };

  return { state, traces, Tags, HMIRuntime, plcScan };
}

function Button_Start_Press(ctx) {
  ctx.Tags("HMI_Start_PB").Write(
    true,
    ctx.HMIRuntime.Tags.Enums.hmiWriteType.hmiWriteWait
  );

  ctx.HMIRuntime.Timers.SetTimeout(function ResetStartPulse() {
    ctx.Tags("HMI_Start_PB").Write(
      false,
      ctx.HMIRuntime.Tags.Enums.hmiWriteType.hmiWriteWait
    );
  }, 200);
}

function Button_Stop_Press(ctx) {
  ctx.Tags("HMI_Stop_PB").Write(
    true,
    ctx.HMIRuntime.Tags.Enums.hmiWriteType.hmiWriteWait
  );

  ctx.HMIRuntime.Timers.SetTimeout(function ResetStopPulse() {
    ctx.Tags("HMI_Stop_PB").Write(
      false,
      ctx.HMIRuntime.Tags.Enums.hmiWriteType.hmiWriteWait
    );
  }, 200);
}

function IOField_SetValue_Change(ctx, newValue) {
  ctx.Tags("HMI_SetValue").Write(
    Number(newValue),
    ctx.HMIRuntime.Tags.Enums.hmiWriteType.hmiWriteWait
  );
}

function Task_ReadFeedback_Update(ctx) {
  const ts = ctx.Tags.CreateTagSet([
    "HMI_Run_Cmd",
    "HMI_Lamp_FB",
    "HMI_SetValue",
    "HMI_ActualValue",
  ]);

  ts.Read(ctx.HMIRuntime.Tags.Enums.hmiReadType.hmiReadDirect);

  ctx.HMIRuntime.Trace(
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

function Button_StartAndPreset_Press(ctx) {
  const ts = ctx.Tags.CreateTagSet([
    ["HMI_SetValue", 1200],
    ["HMI_Start_PB", true],
  ]);

  ts.Write(ctx.HMIRuntime.Tags.Enums.hmiWriteType.hmiWriteWait);

  ctx.HMIRuntime.Timers.SetTimeout(function ResetPulse() {
    ctx.Tags("HMI_Start_PB").Write(
      false,
      ctx.HMIRuntime.Tags.Enums.hmiWriteType.hmiWriteWait
    );
  }, 200);
}

function testStartClosedLoop() {
  const ctx = createMockRuntime();
  Button_Start_Press(ctx);
  ctx.plcScan();
  ctx.HMIRuntime.Timers.runAll();
  ctx.plcScan();

  assert.equal(ctx.state.HMI_Run_Cmd, true);
  assert.equal(ctx.state.Q0_0, true);
  assert.equal(ctx.state.HMI_Lamp_FB, true);

  return "case1 start closed loop ok";
}

function testStopClosedLoop() {
  const ctx = createMockRuntime({
    HMI_Run_Cmd: true,
    HMI_Lamp_FB: true,
    Q0_0: true,
  });

  Button_Stop_Press(ctx);
  ctx.plcScan();
  ctx.HMIRuntime.Timers.runAll();
  ctx.plcScan();

  assert.equal(ctx.state.HMI_Run_Cmd, false);
  assert.equal(ctx.state.Q0_0, false);
  assert.equal(ctx.state.HMI_Lamp_FB, false);

  return "case2 stop closed loop ok";
}

function testSetValueClosedLoop() {
  const ctx = createMockRuntime();
  IOField_SetValue_Change(ctx, 1500);
  ctx.plcScan();
  Task_ReadFeedback_Update(ctx);

  assert.equal(ctx.state.HMI_SetValue, 1500);
  assert.equal(ctx.state.HMI_ActualValue, 1500);
  assert.match(ctx.traces[0], /Set=1500, Actual=1500/);

  return "case3 set/actual closed loop ok";
}

function testStartAndPresetBatchWrite() {
  const ctx = createMockRuntime();
  Button_StartAndPreset_Press(ctx);
  ctx.plcScan();
  ctx.HMIRuntime.Timers.runAll();
  ctx.plcScan();
  Task_ReadFeedback_Update(ctx);

  assert.equal(ctx.state.HMI_Run_Cmd, true);
  assert.equal(ctx.state.HMI_SetValue, 1200);
  assert.equal(ctx.state.HMI_ActualValue, 1200);
  assert.match(ctx.traces[0], /Run=true, Lamp=true, Set=1200, Actual=1200/);

  return "case4 batch write closed loop ok";
}

const results = [
  testStartClosedLoop(),
  testStopClosedLoop(),
  testSetValueClosedLoop(),
  testStartAndPresetBatchWrite(),
];

console.log(results.join("\n"));
