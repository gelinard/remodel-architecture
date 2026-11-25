# Contributing

Thanks for your interest! This project is a reference architecture and a template, so the best contributions keep it **small, correct and easy to read**.

## Before you start

- **Questions and ideas:** open an issue with the *Idea or question* form.
- **Bugs:** open an issue with the *Bug report* form.
- **Bigger changes**, such as a new pattern, building block or dependency: open an issue first, so we can agree on the approach before you write code.

## Build and test

You need the .NET 10 SDK and Docker.

```bash
dotnet build    # warnings are errors
dotnet test     # unit, architecture and contract tests (Docker must be running)
```

To run the host, follow the [quick start](README.md#quick-start).

## Guidelines

- **Follow the six rules** in the README. The architecture tests check most of them, so a green `dotnet test` is a good sign.
- **Use the naming conventions:** `…Query`, `…Command`, `…QueryHandler`, `…CommandHandler` and `…Validator`, and ports named `I…Reader` or `I…Writer`. Put each use case in its own folder: `Features/<UseCase>/`.
- **Every adapter passes the contract tests.** A new adapter is one more line in the matching contract test file.
- **Prefer simple code over clever code.** People copy this repository to learn from it.
- **New dependencies:** use the latest stable version that works with .NET 10, only with a permissive licence (MIT, Apache-2.0, BSD or similar). Put the version in `Directory.Packages.props`.
- **Design decisions get a short ADR** in [`docs/adr`](docs/adr/README.md): context, decision and consequences.

## Pull requests

- Keep each pull request focused on one change.
- Write the title as a [conventional commit](https://www.conventionalcommits.org/), for example `feat: add a Minimal API host`. Pull requests are squash-merged, so the title becomes the commit message.
- CI must be green before a pull request can be merged.

Everyone taking part is expected to follow the [Code of Conduct](CODE_OF_CONDUCT.md). By contributing, you agree that your contribution is licensed under the [MIT License](LICENSE).
