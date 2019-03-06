
FROM microsoft/dotnet:2.2-sdk as build
WORKDIR /src
COPY . ./
RUN sh ./build.sh linux-x64 latest

FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app
COPY --from=build /src/out .
EXPOSE 5000/tcp
ENTRYPOINT dotnet IdentityBase.Web.dll