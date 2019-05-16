FROM mcr.microsoft.com/dotnet/core/sdk:2.2 as build
WORKDIR /IdentityBase

# don't judge me, need to generate this crap automatically
COPY submodules/ServiceBase/src/ServiceBase/ServiceBase.csproj submodules/ServiceBase/src/ServiceBase/
COPY submodules/ServiceBase/src/ServiceBase.Events.RabbitMQ/ServiceBase.Events.RabbitMQ.csproj submodules/ServiceBase/src/ServiceBase.Events.RabbitMQ/
COPY submodules/ServiceBase/src/ServiceBase.Mvc/ServiceBase.Mvc.csproj submodules/ServiceBase/src/ServiceBase.Mvc/
COPY submodules/ServiceBase/src/ServiceBase.Notification.Plivo/ServiceBase.Notification.Plivo.csproj submodules/ServiceBase/src/ServiceBase.Notification.Plivo/
COPY submodules/ServiceBase/src/ServiceBase.Notification.SendGrid/ServiceBase.Notification.SendGrid.csproj submodules/ServiceBase/src/ServiceBase.Notification.SendGrid/
COPY submodules/ServiceBase/src/ServiceBase.Notification.Smtp/ServiceBase.Notification.Smtp.csproj submodules/ServiceBase/src/ServiceBase.Notification.Smtp/
COPY submodules/ServiceBase/src/ServiceBase.Notification.Twilio/ServiceBase.Notification.Twilio.csproj submodules/ServiceBase/src/ServiceBase.Notification.Twilio/
COPY src/IdentityBase.Web/IdentityBase.Web.csproj src/IdentityBase.Web/
COPY src/IdentityBase.Shared/IdentityBase.Shared.csproj src/IdentityBase.Shared/
COPY src/IdentityBase.EntityFramework/IdentityBase.EntityFramework.csproj src/IdentityBase.EntityFramework/
COPY src/IdentityBase.Web/Plugins/IdentityBase.EntityFramework.InMemory/IdentityBase.EntityFramework.InMemory.csproj src/IdentityBase.Web/Plugins/IdentityBase.EntityFramework.InMemory/
COPY src/IdentityBase.Web/Plugins/IdentityBase.EntityFramework.MySql/IdentityBase.EntityFramework.MySql.csproj src/IdentityBase.Web/Plugins/IdentityBase.EntityFramework.MySql/
COPY src/IdentityBase.Web/Plugins/IdentityBase.EntityFramework.Npgsql/IdentityBase.EntityFramework.Npgsql.csproj src/IdentityBase.Web/Plugins/IdentityBase.EntityFramework.Npgsql/
COPY src/IdentityBase.Web/Plugins/IdentityBase.EntityFramework.SqlServer/IdentityBase.EntityFramework.SqlServer.csproj src/IdentityBase.Web/Plugins/IdentityBase.EntityFramework.SqlServer/
COPY src/IdentityBase.Web/Plugins/IdentityBase.EntityFramework.zDbInitializer/IdentityBase.EntityFramework.zDbInitializer.csproj src/IdentityBase.Web/Plugins/IdentityBase.EntityFramework.zDbInitializer/
COPY src/IdentityBase.Web/Plugins/IdentityBase.GoogleRecaptcha/IdentityBase.GoogleRecaptcha.csproj src/IdentityBase.Web/Plugins/IdentityBase.GoogleRecaptcha/
COPY src/IdentityBase.Web/Plugins/IdentityBase.RabbitMq/IdentityBase.RabbitMq.csproj src/IdentityBase.Web/Plugins/IdentityBase.RabbitMq/
COPY src/IdentityBase.Web/Plugins/IdentityBase.SendGrid/IdentityBase.SendGrid.csproj src/IdentityBase.Web/Plugins/IdentityBase.SendGrid/
COPY src/IdentityBase.Web/Plugins/IdentityBase.Smtp/IdentityBase.Smtp.csproj src/IdentityBase.Web/Plugins/IdentityBase.Smtp/
COPY src/IdentityBase.Web/Plugins/IdentityBase.Twilio/IdentityBase.Twilio.csproj src/IdentityBase.Web/Plugins/IdentityBase.Twilio/
COPY src/IdentityBase.Web/Plugins/DefaultTheme/DefaultTheme.csproj src/IdentityBase.Web/Plugins/DefaultTheme/

RUN dotnet restore src/IdentityBase.Web/IdentityBase.Web.csproj
RUN dotnet restore src/IdentityBase.Web/Plugins/IdentityBase.EntityFramework.InMemory/IdentityBase.EntityFramework.InMemory.csproj
RUN dotnet restore src/IdentityBase.Web/Plugins/IdentityBase.EntityFramework.MySql/IdentityBase.EntityFramework.MySql.csproj
RUN dotnet restore src/IdentityBase.Web/Plugins/IdentityBase.EntityFramework.Npgsql/IdentityBase.EntityFramework.Npgsql.csproj
RUN dotnet restore src/IdentityBase.Web/Plugins/IdentityBase.EntityFramework.SqlServer/IdentityBase.EntityFramework.SqlServer.csproj
RUN dotnet restore src/IdentityBase.Web/Plugins/IdentityBase.EntityFramework.zDbInitializer/IdentityBase.EntityFramework.zDbInitializer.csproj
RUN dotnet restore src/IdentityBase.Web/Plugins/IdentityBase.GoogleRecaptcha/IdentityBase.GoogleRecaptcha.csproj
RUN dotnet restore src/IdentityBase.Web/Plugins/IdentityBase.RabbitMq/IdentityBase.RabbitMq.csproj
RUN dotnet restore src/IdentityBase.Web/Plugins/IdentityBase.SendGrid/IdentityBase.SendGrid.csproj
RUN dotnet restore src/IdentityBase.Web/Plugins/IdentityBase.Smtp/IdentityBase.Smtp.csproj
RUN dotnet restore src/IdentityBase.Web/Plugins/IdentityBase.Twilio/IdentityBase.Twilio.csproj
RUN dotnet restore src/IdentityBase.Web/Plugins/DefaultTheme/DefaultTheme.csproj

COPY . ./
RUN ["chmod", "+x", "./build.sh"]
RUN ["./build.sh", "linux-x64", "latest"]

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
COPY --from=build /IdentityBase/artifacts/linux-x64/identitybase-latest /usr/local/identitybase
RUN ["chmod", "+x", "/usr/local/identitybase/run.sh"]
EXPOSE 5000/tcp
VOLUME ["/usr/local/identitybase/config", "/var/log/identitybase"]
ENTRYPOINT ["/usr/local/identitybase/run.sh"]