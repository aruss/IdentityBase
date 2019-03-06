FROM microsoft/dotnet:2.2-sdk as build
WORKDIR /src
COPY *.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -r linux-x64 -o out

FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app
COPY --from=build /src/out .
EXPOSE 5002/tcp
ENTRYPOINT ["dotnet", "AspNetCoreWeb.dll"]