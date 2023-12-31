FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app
COPY API/Grassroots.Identity.API/Grassroots.Identity.API.csproj API/Grassroots.Identity.API/
COPY Common/Grassroots.Identity.Common.DependencyContainer/Grassroots.Identity.Common.DependencyContainer.csproj Common/Grassroots.Identity.Common.DependencyContainer/
COPY Common/Grassroots.Identity.Common/Grassroots.Identity.Common.csproj Common/Grassroots.Identity.Common/
COPY API/Grassroots.Identity.API.ServiceInterface/Grassroots.Identity.API.ServiceInterface.csproj API/Grassroots.Identity.API.ServiceInterface/
COPY API/Grassroots.Identity.API.ServiceModel/Grassroots.Identity.API.ServiceModel.csproj API/Grassroots.Identity.API.ServiceModel/

# REMOVED ServiceStack.csproj PROJECT
# COPY Common/Grassroots.Identity.Common.ServiceStack/Grassroots.Identity.Common.ServiceStack.csproj Common/Grassroots.Identity.Common.ServiceStack/

COPY NuGet.config .
RUN dotnet restore API/Grassroots.Identity.API/Grassroots.Identity.API.csproj --configfile NuGet.config
COPY . .

#RUN dotnet build and publish output in out directory
RUN dotnet build API/Grassroots.Identity.API/Grassroots.Identity.API.csproj
RUN dotnet publish API/Grassroots.Identity.API/Grassroots.Identity.API.csproj -c Release -p:PublishDir=out

# Build runtime image for dotnetcore && nginx && ssh
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS base
RUN apt-get update && apt-get install -y gnupg2 && apt-get install -y wget
RUN echo "deb http://nginx.org/packages/mainline/debian/ stretch nginx" >> /etc/apt/sources.list
RUN wget http://nginx.org/keys/nginx_signing.key
RUN apt-key add nginx_signing.key
RUN apt update
RUN apt-get install -y nginx
RUN apt-get install nano
RUN apt-get install net-tools
RUN rm /etc/nginx/nginx.conf
COPY nginx.conf /etc/nginx/
COPY proxy.conf /etc/nginx/
#COPY default /etc/nginx/sites-available/default

WORKDIR /app
COPY --from=build /app/API/Grassroots.Identity.API/out .

#SSH CONFIG
RUN apt update \
  && apt install -y --no-install-recommends openssh-server \
  && mkdir -p /run/sshd \
  && echo "root:Docker!" | chpasswd
COPY sshd_config /etc/ssh/sshd_config

# Forward request logs to Docker log collector
RUN ln -sf /dev/stdout /var/log/nginx/access.log \
  && ln -sf /dev/stderr /var/log/nginx/error.log

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000 80 2222
ENTRYPOINT ["/bin/bash", "-c", "/usr/sbin/sshd && nginx && dotnet Grassroots.Identity.API.dll "]