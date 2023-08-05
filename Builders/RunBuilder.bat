@echo off

echo Running builder
goto TRY

:TRY
python3 .\Builder.py
if  errorlevel 1 goto TRY2
goto FINISH

:TRY2
echo Path to python3 wasn't found, using default
python .\Builder.py
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