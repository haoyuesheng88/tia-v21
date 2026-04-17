# tia-v21

Portable `TIA Portal V21` skill package for Codex-based automation.

This repository is organized so it can be reused on another computer with minimal setup:

- `skills/tia-openness-closed-loop/`
  The Codex skill itself.
- `examples/`
  Reusable example PLC and Unified script files.
- `tools/openness-src/`
  C# source files for the Openness helpers used during live TIA work.
- `docs/`
  Short installation and usage notes for another machine.

## Install on another computer

1. Copy `skills/tia-openness-closed-loop` into:
   - Windows: `%USERPROFILE%\\.codex\\skills\\`
2. Keep the `examples` and `tools` folders anywhere convenient, for example in a working folder such as `Documents\\Codex\\tia-v21`.
3. Ensure `TIA Portal V21 Openness` is installed and the Windows user is in the `Siemens TIA Openness` group.
4. Build any required helper in `tools/openness-src` with the local Siemens Openness assemblies.

## Included examples

- `examples/plc/1215C_HMI_ClosedLoop_Full.scl`
  Simple PLC/HMI input-output closed loop using absolute addresses.
- `examples/plc/G2_Baghouse_PulseCollector.scl`
  `CPU 1212FC` baghouse pulse dust collector example.
- `examples/unified/`
  WinCC Unified V21 module import package and local mock test files.

## Notes

- The repository contains source material intended for reuse, not the original customer project archive.
- Compiled `.exe` outputs and transient TIA project files are intentionally excluded.
