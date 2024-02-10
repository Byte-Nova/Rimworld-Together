@echo off

echo Running builder
goto TRY

:TRY
py ".\Simple Builder.py"
if  errorlevel 1 goto TRY2
goto FINISH

:TRY2
echo Path to python3 wasn't found, using default
py ".\Simple Builder.py"
if  errorlevel 1 goto TRY3
goto FINISH

:TRY3
echo Default path wasn't found, using "py" path
py ".\Simple Builder.py"
goto FINISH

:FINISH
if %ERRORLEVEL% == 0 (
 goto SUCCESS
) else (
 goto ERROR
)

:ERROR
echo An error occurred while running builder
pause
goto :EOF

:SUCCESS
echo Build Finished
pause
goto :EOF