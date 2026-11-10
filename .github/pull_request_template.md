<!--
  Thanks for contributing to GtkSharp.
  Read CONTRIBUTING.md before opening this pull request.
-->

## What changed

<!-- A short description of the change. -->

## Why

<!-- The problem this solves. Link the issue with "Fixes #123" when possible. -->

## Type of change

- [ ] Bug fix
- [ ] New feature
- [ ] Binding / API update
- [ ] Workload or packaging
- [ ] Documentation
- [ ] Test or CI only

## Tests

- [ ] I added tests that cover this change.
- [ ] `xvfb-run -a dotnet cake build.cake --BuildTarget=Test` passes locally.
- [ ] The change affects the workload and `arm64-smoke` is expected to cover it.

How to run the tests:

```sh
dotnet tool restore
xvfb-run -a dotnet cake build.cake --BuildTarget=Test
```

### Why no tests

<!--
  Required when the "I added tests" box is not checked.
  Explain what cannot be automated, why, and the manual verification you did.
  Delete this section when tests were added.
-->

## Checklist

- [ ] The pull request targets `develop`.
- [ ] No `BuildOutput/`, `bin/`, `obj/` or generated sources are committed.
- [ ] Public API changes are reflected in the tests and documentation.
- [ ] The PR description explains what changed and why.
