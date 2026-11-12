FROM kjanat/actionlint:1.17.1@sha256:c0a63b3e5c974c2466ead39404b70e448176beaff8c5ec7af0b2e96191aea661 AS binaries
FROM golang:1.27.3-alpine@sha256:dbaa054703263d06c8b49576667418473b6231f8c0fa69dee103207fdb8e3654 AS builder
# The released binary embeds Go 1.27.0, which has outstanding OSV advisories.
ADD --checksum=sha256:78def64642f4c85bf32a1a9ffc73b67fd5378a022bb8e3bd918d8fe42282729d https://github.com/kjanat/actionlint/archive/refs/tags/v1.17.0.tar.gz /tmp/source.tar.gz
RUN mkdir /source && tar -xzf /tmp/source.tar.gz -C /source --strip-components=1
WORKDIR /source
RUN CGO_ENABLED=0 GOMEMLIMIT=512MiB GOMAXPROCS=2 go build -p 1 -trimpath -ldflags='-s -w' -o /actionlint ./cmd/actionlint
FROM python:3.16.0a1-alpine@sha256:57d59f9badcd47eabb3ed920d3bd5ae02f44664fc224ae57f1687980227741d2
COPY --from=builder /actionlint /usr/local/bin/actionlint
COPY --from=binaries /usr/local/bin/shellcheck /usr/local/bin/shellcheck
COPY scripts/requirements.lock.txt /tmp/requirements.lock.txt
RUN pip install --no-cache-dir --upgrade --constraint /tmp/requirements.lock.txt pip pyflakes
WORKDIR /work
ENTRYPOINT ["actionlint"]
