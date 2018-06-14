FROM microsoft/dotnet:2.1-aspnetcore-runtime

COPY ./identitybase-2.0.0 /usr/local/identitybase

RUN ["chmod", "+x", "/usr/local/identitybase/run.sh"]

EXPOSE 5000/tcp

VOLUME ["/usr/local/identitybase/config", "/var/log/identitybase"]

ENTRYPOINT ["/usr/local/identitybase/run.sh"]