# Tasks

## 1. Update OpenSpec Configuration

- [x] 1.1 Populate `openspec/config.yaml` with project `context`, artifact `rules` (proposal, specs, design, tasks), and workflow `operations` (apply, archive), and verify the file parses as valid YAML.

## 2. Validation & Verification

- [x] 2.1 Run `openspec doctor` and `openspec validate customize-openspec-config` to verify OpenSpec root integrity and change schema consistency.
- [x] 2.2 Test instruction enrichment by running `openspec instructions design --change "customize-openspec-config" --json` to verify that `context` and `rules` are properly injected.

## Workflow follow-up

- Run `/openspec-apply-change` to apply the configuration change to `openspec/config.yaml`.
- Run `/openspec-archive-change` after verification is complete.
