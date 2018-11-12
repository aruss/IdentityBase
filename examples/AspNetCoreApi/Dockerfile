FROM microsoft/dotnet:2.1-runtime
COPY ./bin/Release/netcoreapp2.1/linux-x64/publish /usr/local/aspnetcoreapi
EXPOSE 3721/tcp
WORKDIR /usr/local/aspnetcoreapi
ENTRYPOINT ["dotnet", "AspNetCoreApi.dll"]