@echo off
REM ==========================================================================
REM == Setup Environment Variables (EXAMPLE TEMPLATE)                       ==
REM ==========================================================================
REM ==                                                                      ==
REM == PURPOSE:                                                             ==
REM == Sets the necessary environment variables for the project in your     ==
REM == Windows user account PERMANENTLY (until you change or delete         ==
REM == them manually or with another script).                               ==
REM ==                                                                      ==
REM == HOW TO USE:                                                          ==
REM == 1. Copy this file and rename the copy to "SetupEnv.bat".             ==
REM ==                                                                      ==
REM == 2. !!!!! IMPORTANT !!!!!                                             ==
REM ==    **ADD "SetupEnv.bat" TO YOUR .gitignore FILE!** ==
REM ==    This will prevent accidentally committing your secrets            ==
REM ==    (especially GITHUB_MLS_PAT) to the Git repository.                ==
REM ==                                                                      ==
REM == 3. Open "SetupEnv.bat" in a text editor and replace the              ==
REM ==    placeholder values (e.g., "YOUR_GITHUB_USERNAME_HERE",            ==
REM ==    "YOUR_GITHUB_PAT_HERE") with your actual data and secrets.        ==
REM ==    Save the file.                                                    ==
REM ==                                                                      ==
REM == 4. Run "SetupEnv.bat" (by double-clicking or from the command line). ==
REM ==    Administrator rights might be needed if you want to use the       ==
REM ==    /M switch for system-wide variables (not recommended for          ==
REM ==    secrets). Without /M, variables are set for the current user.     ==
REM ==                                                                      ==
REM == 5. !!!!! VERY IMPORTANT !!!!!                                        ==
REM ==    Changes made by the SETX command will only take effect in NEW     ==
REM ==    command prompt windows (cmd, PowerShell) or in new processes      ==
REM ==    launched AFTER this script has successfully executed.             ==
REM ==    The current window will NOT update the variables automatically.   ==
REM ==    Restart your terminal or IDE after running the script.            ==
REM ==========================================================================

echo Setting permanent environment variables for the current user...
echo.

REM --- Path to local packages ---
REM Calculate the absolute path relative to this script's location (%~dp0).
REM Assuming the target dir is '../BuildingBlocks/Packages/MarineLaceSpacePackages'
REM relative to the script's directory.
echo Calculating absolute path for MLS_LOCAL_PACKAGE_PATH based on script location...
for %%i in ("%~dp0..\BuildingBlocks\Packages\MarineLaceSpacePackages") do set "ABS_MLS_PACKAGE_PATH=%%~fi"

echo Setting MLS_LOCAL_PACKAGE_PATH to: "%ABS_MLS_PACKAGE_PATH%"
setx MLS_LOCAL_PACKAGE_PATH "%ABS_MLS_PACKAGE_PATH%"
REM Optional: Clear the temporary variable used for calculation
set "ABS_MLS_PACKAGE_PATH="

echo.

REM --- GitHub Credentials ---
REM REPLACE the value in double quotes with YOUR ACTUAL DATA in the SetupEnv.bat file
echo Setting GITHUB_MLS_USER (REPLACE PLACEHOLDER IN SetupEnv.bat!)...
setx GITHUB_MLS_USER "YOUR_GITHUB_USERNAME_HERE"

echo.

REM REPLACE the value in double quotes with YOUR ACTUAL PAT in the SetupEnv.bat file
echo Setting GITHUB_MLS_PAT (REPLACE PLACEHOLDER IN SetupEnv.bat!)...
setx GITHUB_MLS_PAT "YOUR_GITHUB_PAT_HERE"

echo.
echo ==========================================================================
echo Done! Attempt to set environment variables finished.
echo.
echo **REMEMBER:**
echo 1. Check above for any error messages.
echo 2. **CLOSE THIS WINDOW AND OPEN A NEW COMMAND PROMPT WINDOW**
echo    (or restart your IDE/application) to start using the newly set
echo    environment variables.
echo 3. Make sure the "SetupEnv.bat" file (with your secrets)
echo    has been added to `.gitignore`.
echo ==========================================================================
echo.
pause