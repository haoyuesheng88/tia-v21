# Verified Workflow

This reference captures the validated troubleshooting path from a live `TIA Portal V21` project containing:

- `PLC_1 [CPU 1215C DC/DC/DC]`
- `G2_PLC_1 [CPU 1212FC DC/DC/DC]`
- `HMI_1 [KTP1200 Basic PN]`
- `HMI_2 [MTP1200 Unified Comfort]`
- `HMI_3 [TP1200 Comfort]`

## What Worked Reliably

### 1. Fix PLC-side design first

The root cause was not only in HMI tags. PLC symbolic references such as:

- `DB_HMI.Start_PB`
- `DB_HMI.Stop_PB`

caused downstream HMI binding errors. Replacing them with absolute addresses fixed the foundation.

Recommended mapping:

- `HMI_Start_PB` -> `%M10.0`
- `HMI_Stop_PB` -> `%M10.1`
- `HMI_Run_Cmd` -> `%M10.2`
- `HMI_Lamp_FB` -> `%M10.3`
- `HMI_SetValue` -> `%MW20`
- `HMI_ActualValue` -> `%MW22`

### 2. Unified can be handled cleanly by Openness

Validated pattern:

- one PLC connection, for example `HMI_Connection_1`
- driver `SIMATIC S7 1200/1500`
- absolute addresses for tags

### 3. Classic KTP1200 needs extra caution

Observed behavior:

- the `连接` editor showed a real connection row to `PLC_1`
- `Openness` still reported `COUNT|0` for `HMI_RT_1.Connections`

Implication:

- do not block on the object model alone
- verify connection presence in the editor
- use tag table export/import and UI-assisted recovery if needed

### 4. Classic tag export format

Classic HMI tag export may use a `.xlsx` filename while the file content is XML like:

```xml
<Hmi.Tag.TagTable>
  ...
</Hmi.Tag.TagTable>
```

Always inspect file content before assuming workbook tooling.

### 5. Unified script module workflow

Validated pattern:

1. Create or export one real global module under `HMI_2 -> 脚本`.
2. Export the module to inspect the real `.hmi.yml + .hmi.js` structure.
3. Import a prepared module such as `ClosedLoopModule`.
4. Re-export the imported module and verify the functions are present.
5. Only after that, bind screen objects or tasks to those functions.

### 6. 1212FC process example workflow

Validated pattern:

1. Target `G2_PLC_1 [CPU 1212FC]`.
2. Inject a full source file containing:
   - `FB_BAGHOUSE_PULSE`
   - `DB_BAGHOUSE_PULSE`
   - `Main`
3. Compile immediately after generation.
4. Create a tag table for the example I/O addresses.

Observed result:

- block generation succeeded
- compile finished with `0` errors and `1` warning
- the resulting blocks were visible in the project

## Practical Recovery Strategy

1. Export the classic tag table.
2. Preserve the exact connection name found in export, for example `HMI_连接_1`.
3. Remove stale experimental tags or rebuild the table cleanly.
4. Prefer absolute addressing and re-validate from compile output.

## Trigger Phrases This Skill Should Cover

- “连接博图中的 PLC 和屏”
- “用 Openness 自动写入”
- “1215C + KTP1200 + Unified 做闭环”
- “把 SCL 和 HMI 变量自动灌进去”
- “修复 classic HMI 和 Unified 的标签错误”
- “把 Unified Comfort 的脚本直接输进去”
