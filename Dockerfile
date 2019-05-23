FROM mcr.microsoft.com/dotnet/core/sdk:2.2 as build
WORKDIR /IdentityBase

COPY . ./
RUN ["chmod", "+x", "./build.sh"]
RUN ["./build.sh", "linux-x64", "latest"]

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
COPY --from=build /IdentityBase/artifacts/linux-x64/identitybase-latest /usr/local/identitybase
RUN ["chmod", "+x", "/usr/local/identitybase/run.sh"]
EXPOSE 5000/tcp
VOLUME ["/usr/local/identitybase/config", "/var/log/identitybase"]
ENTRYPOINT ["/usr/local/identitybase/run.sh"]