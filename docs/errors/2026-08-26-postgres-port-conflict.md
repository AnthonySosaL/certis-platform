# Postgres port 5432 conflict with an unrelated local service

**Date:** 2026-08-26
**Area:** infra (docker-compose.yml)

## Symptom

`dotnet ef database update` failed with `28P01: password authentication
failed for user "nutriboost"`, even though the password was correct and
`psql` inside the container worked fine.

## Root cause

This machine already has another PostgreSQL server bound to host port 5432
— a native `postgres.exe` Windows service, unrelated to this project (there
are other local projects on this machine, e.g. a `prodigy-*` Docker Compose
stack, that also expect 5432). `netstat` showed **two** listeners on
`0.0.0.0:5432` at once. Connections from `dotnet ef` were landing on the
wrong Postgres server — one that has no `nutriboost` user — while `docker
exec ... psql` connected to the right one from inside the container, which
bypasses host port routing entirely and so never showed the conflict.

## Fix

Changed `docker-compose.yml` to publish this project's Postgres on host
port **5433** instead of 5432 (`"5433:5432"`), and updated
`client-backend/src/NutriBoost.Client.Api/appsettings.Development.json`'s
connection string and `client-frontend/.env.example` to match. Left the
pre-existing service on 5432 alone — it may be load-bearing for another
project on this machine, and touching it wasn't this project's call to
make.

## How to avoid it again

Before trusting "container won't connect" errors as an app-level bug,
check `netstat -ano | grep ":<port>"` for more than one listener on the
same port first — especially on a dev machine that runs multiple local
projects. If Postgres (or any service) refuses to bind on 5432/3306/6379/
etc., assume a collision until proven otherwise, not a config bug.
