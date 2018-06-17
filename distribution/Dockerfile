# code does not run on alpine image yet
FROM microsoft/dotnet:2.1-runtime

ARG VERSION

COPY ./identitybase-$VERSION /usr/local/identitybase

RUN ["chmod", "+x", "/usr/local/identitybase/run.sh"]

EXPOSE 5000/tcp

VOLUME ["/usr/local/identitybase/config", "/var/log/identitybase"]

ENTRYPOINT ["/usr/local/identitybase/run.sh"]