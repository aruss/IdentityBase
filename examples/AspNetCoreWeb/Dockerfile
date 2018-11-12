FROM microsoft/dotnet:2.1-runtime
COPY ./bin/Release/netcoreapp2.1/linux-x64/publish /usr/local/aspnetcoreweb
EXPOSE 21402/tcp
WORKDIR /usr/local/aspnetcoreweb
ENTRYPOINT ["dotnet", "AspNetCoreWeb.dll"]