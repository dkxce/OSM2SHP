;
; OSM2SHP Installer
;

Name "OSM2SHP 21.10.19.36"

; The file to write
OutFile "_INSTALLER\OSM2SHP Fastex Converter Setup 21.10.19.36.exe"
Icon "installer.ico"

; The default installation directory
InstallDir $PROGRAMFILES\OSM2SHP

; Request application privileges for Windows Vista
; RequestExecutionLevel admin
RequestExecutionLevel user

; Registry key to check for directory (so if you install again, it will 
; overwrite the old one automatically)
InstallDirRegKey HKLM "Software\OSM2SHP" "Install_Dir"


;--------------------------------

; Pages

Page directory
Page instfiles

UninstPage uninstConfirm
UninstPage instfiles

;installers

Section "Install"
  WriteRegStr HKLM SOFTWARE\OSM2SHP "Install_Dir" "$INSTDIR"  
  
  SetOutPath $INSTDIR    
  File "_empty.dbf"  
  File "_test.dbf"  
  File "*.dll"    
  File "*.txt"    
  File "*.exe"    
  File "*.cmd"    
  File "*.pdf"    
  
  SetOutPath $INSTDIR\dbf_editor
  File /r "dbf_editor\*.*"   
  
  SetOutPath $INSTDIR\json_addr
  File /r "json_addr\*.*"
  
  SetOutPath $INSTDIR\fonts
  File /r "fonts\*.*"
  
  SetOutPath $INSTDIR\json_garmin_points
  File /r "json_garmin_points\*.*"
  
  SetOutPath $INSTDIR\json_mp_points
  File /r "json_mp_points\*.*"
  
  SetOutPath $INSTDIR\json_poi_catalog
  File /r "json_poi_catalog\*.*"
  
  SetOutPath $INSTDIR\xml_selector
  File /r "xml_selector\*.*"
  
  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\OSM2SHP" "DisplayName" "OSM2SHP Converter"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\OSM2SHP" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteUninstaller "uninstall.exe"
  
  ; Shortcuts
  CreateDirectory "$SMPROGRAMS\OSM2SHP Converter"
  CreateShortcut "$SMPROGRAMS\OSM2SHP Converter\OSM2SHP Fastex Converter.lnk" "$INSTDIR\OSM2SHPFC.exe" "" "$INSTDIR\OSM2SHPFC.exe" 0  
  CreateShortcut "$SMPROGRAMS\OSM2SHP Converter\Uninstall.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0    
SectionEnd

; Uninstaller

Section "Uninstall"  
  RMDir /r "$SMPROGRAMS\OSM2SHP Converter"
  RMDir /r $INSTDIR  
SectionEnd
