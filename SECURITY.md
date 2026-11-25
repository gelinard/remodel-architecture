# Security policy

## Supported versions

This is a reference architecture and a template. Security fixes go to the latest version on the `main` branch.

## Reporting a vulnerability

Please don't open a public issue for a security problem.

Report it privately with GitHub's private vulnerability reporting: open the [Security tab](https://github.com/gelinard/remodel-architecture/security) and click **Report a vulnerability**.

Describe what you found, how to reproduce it, and what an attacker could do with it. This is a personal project, so answers are best-effort, but security reports come first.

## Not a vulnerability

The sample uses well-known passwords in `docker-compose.yml` and `local.settings.json`. They exist only for the local Docker containers. Never use them outside local development.
