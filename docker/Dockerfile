# This dockerfile is used to build a docker image for the builds website.
FROM mcr.microsoft.com/dotnet/sdk:3.1 as build

# Clone and build the website.
#RUN git clone --depth 1 https://github.com/APSIMInitiative/APSIM.Builds /APSIM.Builds
#RUN cd /APSIM.Builds/src/APSIM.Builds && dotnet publish -c Release -f net6.0 -r linux-x64 --no-self-contained
RUN git clone --depth 1 https://github.com/hol430/APSIM.Builds.Migrator /migrator
RUN cd /migrator && dotnet publish -c Release -f netcoreapp3.1 -r linux-x64 --no-self-contained

# Actual container is based on dotnet/runtime, and doesn't include build tools.
FROM mcr.microsoft.com/dotnet/runtime:3.1-bullseye-slim
RUN mkdir -p /opt/migrator
COPY --from=build /migrator/bin/Release/netcoreapp3.1/linux-x64/publish/ /opt/migrator/
SHELL ["/bin/bash"]
WORKDIR "/opt/migrator"
ENTRYPOINT ["/opt/migrator/APSIM.Builds.Migrator"]
