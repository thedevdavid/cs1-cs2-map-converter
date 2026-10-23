# The runner owns the step order; the converter only declares files

The import runner (Node) decides which steps run, in what order, and with which bridge rules (per-endpoint options, stamp caps, version support), because every one of those reasons is bridge or game behaviour learned from attempts (e.g. surfaces before networks since A-v4, where a surface import removed 309 built segments). The converter (C#) writes an import manifest per scale set that only says which import files belong to which step; it carries no order and no bridge options. A manifest that looks like a run plan would invite moving the order into the converter, which would split bridge knowledge across the C#/Node seam again.

## Considered Options

- Converter's manifest is the full ordered run plan and the runner walks it: rejected, the order's reasons are bridge behaviour and would then live away from the bridge client.
