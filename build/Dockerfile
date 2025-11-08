FROM mcr.microsoft.com/dotnet/sdk:9.0.306-alpine3.22@sha256:85f8507e10f3bf6b1a13e02ab0630918be3993b30bd7706774b77d8b0d1a59ed AS builder
RUN apk add --no-cache clang build-base zlib-dev icu-libs

ENV APPNAME=jdc.build.dependencyGraph
ENV BUILD_CONFIGURATION=Release
ENV BUILD_PROJECT=/src/$APPNAME/$APPNAME.csproj
ENV TARGETRUNTIME=linux-musl-x64

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0
ENV DOTNET_SYSTEM_GLOBALIZATION_USENLS=false

COPY src /src

WORKDIR /

RUN dotnet restore ${BUILD_PROJECT} --runtime ${TARGETRUNTIME}
RUN dotnet publish ${BUILD_PROJECT} --runtime ${TARGETRUNTIME} -c ${BUILD_CONFIGURATION} --no-restore --self-contained true -p:DebugSymbols=false
RUN dotnet publish ${BUILD_PROJECT} --runtime ${TARGETRUNTIME} -c ${BUILD_CONFIGURATION} -o /app --no-restore --no-build --self-contained true -p:DebugSymbols=false

###### ACTUAL CONTAINER ######

FROM mcr.microsoft.com/dotnet/runtime-deps:9.0.10-alpine3.22@sha256:619ccecf37596f0c875c02472e845cf4d87f1557c75b2f9eeeb772b099f7527f AS final
LABEL org.opencontainers.image.description="A simple tool to build a simple mermaid graph with dependency of a dotnet project."
LABEL org.opencontainers.image.authors="https://github.com/j-dc"
LABEL org.opencontainers.image.url="https://ghcr.io/j-dc/jdc-build-dependencygraph"

RUN apk add --no-cache icu-libs

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0
ENV DOTNET_SYSTEM_GLOBALIZATION_USENLS=false


COPY --from=builder /app /app
WORKDIR /app
RUN mkdir /src

USER app:app
#ENTRYPOINT [ "/app/jdc.build.dependencyGraph" ]
cmd ["sleep", "infinity"]
