# Dust Collector Example

This reference captures a validated `Openness` workflow for a `CPU 1212FC` project using a baghouse pulse dust collector example.

## Target Shape

Use an isolated implementation:

- `FB_BAGHOUSE_PULSE`
- `DB_BAGHOUSE_PULSE`
- call from `Main`

This reduces risk to the existing user program.

## Example I/O Mapping

- `StartCmd` -> `%I0.0`
- `StopCmd` -> `%I0.1`
- `DiffPressHigh` -> `%I0.2`
- `PulseFeedback` -> `%I0.3`
- `FanRun` -> `%Q0.0`
- `Valve1` -> `%Q0.1`
- `Valve2` -> `%Q0.2`
- `Valve3` -> `%Q0.3`
- `Valve4` -> `%Q0.4`
- `Alarm` -> `%Q0.5`
- `CycleStep` -> `%MW10`

## Closed-Loop Behavior

- `StartCmd` latches the collector run state
- `StopCmd` resets run and alarm
- `DiffPressHigh` enables pulse cleaning sequence
- valves pulse one by one with timer-based sequencing
- `PulseFeedback` is checked after each pulse
- missing feedback latches `Alarm`

## Validated SCL Pattern

- use `TON` for interval timing
- use a second `TON` for pulse width
- use a `CASE Step OF` state machine for valve rotation
- keep outputs defaulted to `FALSE` every scan, then assert only the active valve

## Recommended Timing Defaults

- `PulseInterval := T#8s`
- `PulseWidth := T#300ms`

Tune them per process later.

## Follow-Up Tag Table

Create a PLC tag table such as `DustCollector_Tags` for operator visibility:

- `DC_Start_Cmd`
- `DC_Stop_Cmd`
- `DC_DiffPress_High`
- `DC_Pulse_Feedback`
- `DC_Fan_Run`
- `DC_Valve_1`
- `DC_Valve_2`
- `DC_Valve_3`
- `DC_Valve_4`
- `DC_Alarm`
- `DC_Cycle_Step`

## Recommended Execution Order

1. Confirm the real PLC is `G2_PLC_1` or other requested CPU name.
2. Inject the source file with `Openness`.
3. Compile immediately.
4. Create a readable PLC tag table for the example addresses.
5. Report the compile state and the resulting I/O loop clearly.
