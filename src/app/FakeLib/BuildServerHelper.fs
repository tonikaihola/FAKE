﻿[<AutoOpen>]
/// Contains functions which allow build scripts to interact with a build server.
module Fake.BuildServerHelper

/// The server type option.
type BuildServer = 
    | TeamFoundation
    | TeamCity
    | CCNet
    | Jenkins
    | Travis
    | AppVeyor
    | GitLabCI
    | LocalBuild

/// The trace mode option.
type TraceMode = 
    | Console
    | Xml

/// Defines if FAKE will use verbose tracing.
/// This flag can be specified by setting the *verbose* build parameter.
let mutable verbose = hasBuildParam "verbose"

/// A constant label for local builds
/// [omit]            
let localBuildLabel = "LocalBuild"

/// Defines the XML output file - used for build servers like CruiseControl.NET.
/// This output file can be specified by using the *logfile* build parameter.
let mutable xmlOutputFile = getBuildParamOrDefault "logfile" "./output/Results.xml"

/// Checks if we are on Team Foundation
/// [omit]
let isTFBuild =
    let tfbuild = environVar "TF_BUILD"
    tfbuild <> null && tfbuild.ToLowerInvariant() = "true"

/// Build number retrieved from Team Foundation
/// [omit]
let tfBuildNumber = environVar "BUILD_BUILDNUMBER"

/// Build number retrieved from TeamCity
/// [omit]
let tcBuildNumber = environVar "BUILD_NUMBER"

/// Build number retrieved from Travis
/// [omit]
let travisBuildNumber = environVar "TRAVIS_BUILD_NUMBER"

/// Checks if we are on GitLab CI
/// [omit]
let isGitlabCI = environVar "CI_SERVER_NAME" = "GitLab CI"

/// Build number retrieved from GitLab CI
/// [omit]
let gitlabCIBuildNumber = if isGitlabCI then environVar "CI_BUILD_ID" else ""

/// Build number retrieved from Jenkins
/// [omit]
let jenkinsBuildNumber = tcBuildNumber

/// CruiseControl.NET Build label
/// [omit]
let ccBuildLabel = environVar "CCNETLABEL"

/// AppVeyor build number
/// [omit]
let appVeyorBuildVersion = environVar "APPVEYOR_BUILD_VERSION"

/// The current build server
let buildServer = 
    if hasBuildParam "JENKINS_HOME" then Jenkins
    elif hasBuildParam "TEAMCITY_VERSION" then TeamCity
    elif not (isNullOrEmpty ccBuildLabel) then CCNet
    elif not (isNullOrEmpty travisBuildNumber) then Travis
    elif not (isNullOrEmpty appVeyorBuildVersion) then AppVeyor
    elif isGitlabCI then GitLabCI
    elif isTFBuild then TeamFoundation
    else LocalBuild

/// The current build version as detected from the current build server.
let buildVersion = 
    let getVersion = getBuildParamOrDefault "buildVersion"
    match buildServer with
    | Jenkins -> getVersion jenkinsBuildNumber
    | TeamCity -> getVersion tcBuildNumber
    | CCNet -> getVersion ccBuildLabel
    | Travis -> getVersion travisBuildNumber
    | AppVeyor -> getVersion appVeyorBuildVersion
    | GitLabCI -> getVersion gitlabCIBuildNumber
    | TeamFoundation -> getVersion tfBuildNumber
    | LocalBuild -> getVersion localBuildLabel

/// Is true when the current build is a local build.
let isLocalBuild = LocalBuild = buildServer
