## WinCC Unified V21 script conclusions

- `WinCC Unified V21` runtime scripting language is `JavaScript`.
- Scripts are supported on `Screen`, `Screen object`, and `Task`.
- `TagSet.Read()` can read from cache or direct from AS.
- `TagSet.Write()` writes directly to the AS.
- `Tag.Write(value, hmiWriteWait)` is available for single-tag writes.
- `HMIRuntime.Timers.SetTimeout()` is supported, but Siemens notes that `SetInterval` is more stable on Unified Panels in some runtime situations.

## Closed-loop tag set used here

- `HMI_Start_PB`
- `HMI_Stop_PB`
- `HMI_Run_Cmd`
- `HMI_Lamp_FB`
- `HMI_SetValue`
- `HMI_ActualValue`

## Tested local mock cases

1. Start pulse -> PLC latch -> output on -> lamp feedback on
2. Stop pulse -> PLC unlatch -> output off -> lamp feedback off
3. Set value input -> PLC copy -> actual value feedback
4. Batch write start + preset -> run feedback + numeric feedback

## Local files

- `unified_v21_closed_loop_examples.js`
- `unified_v21_closed_loop_mock_test.js`
