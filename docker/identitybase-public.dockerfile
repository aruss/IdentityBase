FROM microsoft/dotnet:runtime
WORKDIR /app
COPY app .
EXPOSE 5000
ENTRYPOINT ["dotnet", "IdentityBase.Public.dll"]