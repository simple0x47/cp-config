FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 443
COPY ./app/. .
RUN apt-get update && \
    apt-get install -y curl unzip build-essential && \
    curl -LO https://github.com/bitwarden/sdk/releases/download/bws-v0.3.0/bws-x86_64-unknown-linux-gnu-0.3.0.zip && \
    unzip bws-x86_64-unknown-linux-gnu-0.3.0.zip && \
    chmod +x bws && \
    mv ./bws /usr/bin/ && \
    curl -LO https://github.com/microconfig/microconfig/releases/download/v4.9.2/microconfig-linux.zip && \
    unzip microconfig-linux.zip && \
    chmod +x microconfig && \
    mv ./microconfig /usr/bin/
ENTRYPOINT ["dotnet", "Cuplan.Config.dll"]