FROM kjanat/actionlint:1.17.0@sha256:d484cccadf56b6a356466ade9af4c8e05c5dce1badaf0a9ecd0d7639d68d9c92 AS binaries
FROM golang:1.27.3-alpine@sha256:dbaa054703263d06c8b49576667418473b6231f8c0fa69dee103207fdb8e3654 AS builder
# The released binary embeds Go 1.27.0, which has outstanding OSV advisories.
ADD --checksum=sha256:78def64642f4c85bf32a1a9ffc73b67fd5378a022bb8e3bd918d8fe42282729d https://github.com/kjanat/actionlint/archive/refs/tags/v1.17.0.tar.gz /tmp/source.tar.gz
RUN mkdir /source && tar -xzf /tmp/source.tar.gz -C /source --strip-components=1
WORKDIR /source
RUN CGO_ENABLED=0 GOMEMLIMIT=512MiB GOMAXPROCS=2 go build -p 1 -trimpath -ldflags='-s -w' -o /actionlint ./cmd/actionlint
FROM python:3.15.2-alpine@sha256:d499023466546cd88a358f80b46fba4016f32597bc4482bb1c47b483a07d457d
COPY --from=builder /actionlint /usr/local/bin/actionlint
COPY --from=binaries /usr/local/bin/shellcheck /usr/local/bin/shellcheck
COPY scripts/requirements.lock.txt /tmp/requirements.lock.txt
RUN pip install --no-cache-dir --upgrade --constraint /tmp/requirements.lock.txt pip pyflakes
WORKDIR /work
ENTRYPOINT ["actionlint"]
