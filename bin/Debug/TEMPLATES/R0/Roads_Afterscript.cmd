@echo off
rem echo %CD%
rem echo %CNV_PATH%
rem echo %DST_PATH%
rem echo %DST_FILE%
rem echo %DST_PROJ%
rem echo %SRC_FILE%
echo copy default.fldcfg.xml
copy "%CNV_PATH%\Templates\R0\default.fldcfg.xml" "%DST_PATH%\*.*" /y
echo Delete Points File
del "%DST_PATH%\%DST_PROJ%[P*.*
echo Delete Lines Split File
del "%DST_PATH%\%DST_PROJ%[L_*.*
echo Delete M File
del "%DST_PATH%\%DST_PROJ%[M*.*
echo Delete R File
del "%DST_PATH%\%DST_PROJ%[R*.*
echo Update
"%CNV_PATH%\Templates\R0\fart.exe" "%DST_PATH%\default.fldcfg.xml" "[NODES]" "%DST_PROJ%[NODES].shp"
"%CNV_PATH%\Templates\R0\fart.exe" "%DST_PATH%\default.fldcfg.xml" "[J0001]" "%DST_PROJ%[J0001].dbf"
echo DONE