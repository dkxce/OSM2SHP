;
; OSM2SHP Installer
;
Unicode True

; Include
!include "MUI2.nsh"
!include "nsDialogs.nsh"

; PROJECT CONSTANTS 
!define PRODUCT_BUILD "21.12.15.53"
!define PRODUCT_NAME "OSM2SHP Converter"
!define COPYRIGHTS "Milok Zbrozek <milokz@gmail.com>"
!define INSTALLER "OSM2SHP Converter Setup"
!define LockPassword "0"
; ---------------------------------------------------------------------

; PROJECT MAIN INFO
BrandingText "${COPYRIGHTS}"
Caption "Установка ${PRODUCT_NAME} v${PRODUCT_BUILD}"
Name "${PRODUCT_NAME}"
Icon "setup.ico"
InstallDir "$PROGRAMFILES\${PRODUCT_NAME}"
InstallDirRegKey HKLM "Software\${PRODUCT_NAME}" "Install_Dir"
OutFile "_INSTALLER\${INSTALLER} ${PRODUCT_BUILD}.exe"
; ---------------------------------------------------------------------
  
; Addit Params
ShowInstDetails show
RequestExecutionLevel user
; ---------------------------------------------------------------------

; Main Variables
Var StartMenuFolder
Var hCtl_PassBox
Var hCtl_PassBox_pass
Var hCtl_PassBox_Label1
Var PasswordEntered
; ---------------------------------------------------------------------

; Interface Settings
!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP "${NSISDIR}\Contrib\Graphics\Header\orange.bmp" 
!define MUI_ICON "setup.ico"
!define MUI_UNICON "setup.ico"
!define MUI_ABORTWARNING

; Registry Settings
!define MUI_LANGDLL_REGISTRY_ROOT "HKCU" 
!define MUI_LANGDLL_REGISTRY_KEY "Software\${PRODUCT_NAME}" 
!define MUI_LANGDLL_REGISTRY_VALUENAME "Installer Language"
; ---------------------------------------------------------------------

; Start Menu Folder Page Configuration
!define MUI_STARTMENUPAGE_REGISTRY_ROOT "HKCU" 
!define MUI_STARTMENUPAGE_REGISTRY_KEY "Software\${PRODUCT_NAME}" 
!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME "Start Menu Folder"
; ---------------------------------------------------------------------

; FINISH Page Configuration
!define MUI_FINISHPAGE_SHOWREADME ""
!define MUI_FINISHPAGE_SHOWREADME_NOTCHECKED
!define MUI_FINISHPAGE_SHOWREADME_TEXT "Создать ярлык на рабочем столе"
!define MUI_FINISHPAGE_SHOWREADME_FUNCTION finishpageaction
; ---------------------------------------------------------------------

; Pages
!insertmacro MUI_PAGE_LICENSE "License.txt"
Page Custom EnterPasswordDialogShow EnterPasswordDialogLeave
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_STARTMENU Application $StartMenuFolder
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
; ---------------------------------------------------------------------

;Unistall Pages
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
; ---------------------------------------------------------------------

;Languages
!insertmacro MUI_LANGUAGE "Russian"
; ---------------------------------------------------------------------

; Install Types
InstType "Полная" ; 1
InstType "Стандартная" ; 2
InstType "Минимальная" ; 3
InstType "Со всеми DBF Tools" ; 4
InstType "Со всеми Shape Tools" ; 5
; ---------------------------------------------------------------------

Section "!Main"
  SectionIn 1 2 3 4 5 RO
  
  WriteRegStr HKLM "SOFTWARE\${PRODUCT_NAME}" "Install_Dir" "$INSTDIR" 
  
  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" "DisplayName" "${PRODUCT_NAME} ${PRODUCT_BUILD}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}" "UninstallString" '"$INSTDIR\uninstall.exe"'
  
  SetOutPath $INSTDIR   
  WriteUninstaller "uninstall.exe"
  
  SetOutPath $INSTDIR    
  File "_empty.dbf"  
  File "_test.dbf"  
  File "*.dll"    
  File "*.txt"    
  File "*.exe"    
  File "*.cmd"    
  File "*.pdf"  

  SetOutPath $INSTDIR\json_addr
  File /r "json_addr\*.*"
  
  SetOutPath $INSTDIR\json_garmin_points
  File /r "json_garmin_points\*.*"
  
  SetOutPath $INSTDIR\json_mp_points
  File /r "json_mp_points\*.*"
  
  SetOutPath $INSTDIR\json_poi_catalog
  File /r "json_poi_catalog\*.*"
  
  SetOutPath $INSTDIR\xml_selector
  File /r "xml_selector\*.*"  
  
  ; Shortcuts
  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
  CreateDirectory "$SMPROGRAMS\$StartMenuFolder"  
  CreateShortcut "$SMPROGRAMS\$StartMenuFolder\${PRODUCT_NAME} v${PRODUCT_BUILD}.lnk" "$INSTDIR\OSM2SHPFC.exe" "" "$INSTDIR\OSM2SHPFC.exe" 0  
  CreateDirectory "$SMPROGRAMS\$StartMenuFolder\Uninstall"
  CreateShortcut "$SMPROGRAMS\$StartMenuFolder\Uninstall\Uninstall.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0    
  !insertmacro MUI_STARTMENU_WRITE_END  
SectionEnd

Section "!Fonts"
  SectionIn 1 2 3 4 5             
  SetOutPath $INSTDIR\fonts
  File /r "fonts\*.*"   
SectionEnd

Section "!Templates"
  SectionIn 1 2 3 4 5
  SetOutPath $INSTDIR\TEMPLATES
  File /r "TEMPLATES\*.*" 
SectionEnd

Section "BoundsShapeBuilder"
  SectionIn 1 2 5 
  SetOutPath $INSTDIR\BoundsShapeBuilder
  File /r "BoundsShapeBuilder\BoundsShapeBuilder.*" 
  File /r "BoundsShapeBuilder\*.dll" 
  
  ; Shortcuts
  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
  CreateDirectory "$SMPROGRAMS\$StartMenuFolder\Shapes Tools"
  CreateShortcut "$SMPROGRAMS\$StartMenuFolder\Shapes Tools\BoundsShapeBuilder.lnk" "$INSTDIR\BoundsShapeBuilder\BoundsShapeBuilder.exe" "" "$INSTDIR\BoundsShapeBuilder\BoundsShapeBuilder.exe" 0  
  !insertmacro MUI_STARTMENU_WRITE_END  
SectionEnd

Section "ShapesMerger"
  SectionIn 1 5
  SetOutPath $INSTDIR\BoundsShapeBuilder
  File /r "BoundsShapeBuilder\ShapesMerger.*" 
  File /r "BoundsShapeBuilder\*.dll" 
  
  ; Shortcuts
  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
  CreateDirectory "$SMPROGRAMS\$StartMenuFolder\Shapes Tools"
  CreateShortcut "$SMPROGRAMS\$StartMenuFolder\Shapes Tools\ShapesMerger.lnk" "$INSTDIR\BoundsShapeBuilder\ShapesMerger.exe" "" "$INSTDIR\BoundsShapeBuilder\ShapesMerger.exe" 0  
  !insertmacro MUI_STARTMENU_WRITE_END  
SectionEnd

Section "ShapesPolygonsExtractor"
  SectionIn 1 2 5
  SetOutPath $INSTDIR\BoundsShapeBuilder
  File /r "BoundsShapeBuilder\ShapesPolygonsExtractor.*" 
  File /r "BoundsShapeBuilder\*.dll" 
  
  ; Shortcuts
  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
  CreateDirectory "$SMPROGRAMS\$StartMenuFolder\Shapes Tools"
  CreateShortcut "$SMPROGRAMS\$StartMenuFolder\Shapes Tools\ShapesPolygonsExtractor.lnk" "$INSTDIR\BoundsShapeBuilder\ShapesPolygonsExtractor.exe" "" "$INSTDIR\BoundsShapeBuilder\ShapesPolygonsExtractor.exe" 0  
  !insertmacro MUI_STARTMENU_WRITE_END  
SectionEnd

Section "AkelPad"
  SectionIn 1 2
  SetOutPath $INSTDIR\dbf_editor
  File /r "dbf_editor\AkelPad.exe"   
  
  ; Shortcuts
  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
  CreateDirectory "$SMPROGRAMS\$StartMenuFolder\Tools"
  CreateShortcut "$SMPROGRAMS\$StartMenuFolder\Tools\AkelPad.lnk" "$INSTDIR\dbf_editor\AkelPad.exe" "" "$INSTDIR\dbf_editor\AkelPad.exe" 0  
  !insertmacro MUI_STARTMENU_WRITE_END  
SectionEnd

Section /o "CDBFW"
  SectionIn 1 4
  SetOutPath $INSTDIR\dbf_editor\CDBFW
  File /r "dbf_editor\CDBFW\*.*"   
  
  ; Shortcuts
  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
  CreateDirectory "$SMPROGRAMS\$StartMenuFolder\DBF Tools"
  CreateShortcut "$SMPROGRAMS\$StartMenuFolder\DBF Tools\CDBFW.lnk" "$INSTDIR\dbf_editor\CDBFW\cdbfw.exe" "" "$INSTDIR\dbf_editor\CDBFW\cdbfw.exe" 0  
  !insertmacro MUI_STARTMENU_WRITE_END  
SectionEnd

Section /o "DBFShow"
  SectionIn 1 4
  SetOutPath $INSTDIR\dbf_editor\DBFShow
  File /r "dbf_editor\DBFShow\*.*"   
  
  ; Shortcuts
  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
  CreateDirectory "$SMPROGRAMS\$StartMenuFolder\DBF Tools"
  CreateShortcut "$SMPROGRAMS\$StartMenuFolder\DBF Tools\DBFShow.lnk" "$INSTDIR\dbf_editor\DBFShow\DBFShow.exe" "" "$INSTDIR\dbf_editor\DBFShow\DBFShow.exe" 0  
  !insertmacro MUI_STARTMENU_WRITE_END  
SectionEnd

Section /o "winDBFview"
  SectionIn 1 4
  SetOutPath $INSTDIR\dbf_editor\winDBFview
  File /r "dbf_editor\winDBFview\*.*"   
  
  ; Shortcuts
  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
  CreateDirectory "$SMPROGRAMS\$StartMenuFolder\DBF Tools"
  CreateShortcut "$SMPROGRAMS\$StartMenuFolder\DBF Tools\winDBFview.lnk" "$INSTDIR\dbf_editor\winDBFview\wDBFview.exe" "" "$INSTDIR\dbf_editor\winDBFview\wDBFview.exe" 0  
  !insertmacro MUI_STARTMENU_WRITE_END  
SectionEnd

Section /o "DBFCommander"
  SectionIn 1 4
  SetOutPath $INSTDIR\dbf_editor
  File "dbf_editor\DBFCommander*.*"   
  
  ; Shortcuts
  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
  CreateDirectory "$SMPROGRAMS\$StartMenuFolder\DBF Tools"
  CreateShortcut "$SMPROGRAMS\$StartMenuFolder\DBF Tools\DBFCommander.lnk" "$INSTDIR\dbf_editor\DBFCommanderRunner.exe" "" "$INSTDIR\dbf_editor\DBFCommander_v4.4.0.101.exe" 0  
  !insertmacro MUI_STARTMENU_WRITE_END  
SectionEnd

Section "DBFExplorer"
  SectionIn 1 2 4
  SetOutPath $INSTDIR\dbf_editor
  File "dbf_editor\DBFExplorer*.*"   
  
  ; Shortcuts
  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
  CreateDirectory "$SMPROGRAMS\$StartMenuFolder\DBF Tools"
  CreateShortcut "$SMPROGRAMS\$StartMenuFolder\DBF Tools\DBFExplorer.lnk" "$INSTDIR\dbf_editor\DBFExplorer.exe" "" "$INSTDIR\dbf_editor\DBFExplorer.4.0.101.exe" 0  
  !insertmacro MUI_STARTMENU_WRITE_END  
SectionEnd

Section "DBFNavigator"
  SectionIn 1 2 4
  SetOutPath $INSTDIR\dbf_editor
  File "dbf_editor\DBFNavigator*.*"   
  
  ; Shortcuts
  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
  CreateDirectory "$SMPROGRAMS\$StartMenuFolder\DBF Tools"
  CreateShortcut "$SMPROGRAMS\$StartMenuFolder\DBF Tools\DBFNavigator.lnk" "$INSTDIR\dbf_editor\DBFNavigator.exe" "" "$INSTDIR\dbf_editor\DBFNavigator.exe" 0  
  !insertmacro MUI_STARTMENU_WRITE_END  
SectionEnd

Section "SDBF"
  SectionIn 1 2 4
  SetOutPath $INSTDIR\dbf_editor
  File "dbf_editor\SDBF.exe"   
  
  ; Shortcuts
  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
  CreateDirectory "$SMPROGRAMS\$StartMenuFolder\DBF Tools"
  CreateShortcut "$SMPROGRAMS\$StartMenuFolder\DBF Tools\SDBF.lnk" "$INSTDIR\dbf_editor\Sdbf.exe" "" "$INSTDIR\dbf_editor\Sdbf.exe" 0  
  !insertmacro MUI_STARTMENU_WRITE_END  
SectionEnd

Section "Редактор DBF"
  SectionIn 1 4
  SetOutPath $INSTDIR\dbf_editor
  File "dbf_editor\sergdbf*.*"     
  File "dbf_editor\*.a*"     
  File "dbf_editor\*.c*"   
  File "dbf_editor\*.d*"
  
  ; Shortcuts
  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
  CreateDirectory "$SMPROGRAMS\$StartMenuFolder\DBF Tools"            
  CreateShortcut "$SMPROGRAMS\$StartMenuFolder\DBF Tools\Редактор DBF.lnk" "$INSTDIR\dbf_editor\sergdbf.exe" "" "$INSTDIR\dbf_editor\sergdbf.exe" 0  
  !insertmacro MUI_STARTMENU_WRITE_END  
SectionEnd

; Uninstaller

Section "Uninstall"  
  !insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuFolder
  RMDir /r "$SMPROGRAMS\$StartMenuFolder"
  RMDir /r $INSTDIR  
  
  Delete "$DESKTOP\${PRODUCT_NAME} v${PRODUCT_BUILD}.lnk"
SectionEnd

; On Init Function
Function .onInit
    !insertmacro MUI_LANGDLL_DISPLAY
	
	StrCpy $PasswordEntered ${LockPassword}
	
	SetCurInstType 1
	
	; Prevent Multiples Install
	System::Call 'kernel32::CreateMutex(p 0, i 0, t "${PRODUCT_NAME}") p .r1 ?e'
	Pop $R0
	
	; Only One Instance
	StrCmp $R0 0 +3
    MessageBox MB_OK|MB_ICONEXCLAMATION "Программа установки уже запущена!"
    Abort
FunctionEnd
; ---------------------------------------------------------------------

; On Uninstall Init
Function un.onInit
  !insertmacro MUI_UNGETLANGUAGE
FunctionEnd
; ---------------------------------------------------------------------

; desktop shortcuts
Function finishpageaction
	DetailPrint "Create desktop shortcuts"  
	CreateShortcut "$DESKTOP\${PRODUCT_NAME} v${PRODUCT_BUILD}.lnk" "$INSTDIR\OSM2SHPFC.exe" "" "$INSTDIR\OSM2SHPFC.exe" 0  
FunctionEnd
; ---------------------------------------------------------------------

; EnterPassword
Function EnterPasswordDialogShow
  ${If} $PasswordEntered != "0"
    nsDialogs::Create 1018
    Pop $hCtl_PassBox
    ${If} $hCtl_PassBox == error
      Abort
    ${EndIf}
    !insertmacro MUI_HEADER_TEXT "Ввод цифрового ключа" "Введите цифровой ключ для данного ПО"  
    ${NSD_CreateLabel} 21.06u 30.77u 226.43u 11.69u "Для установки данного ПО требуется цифровой ключ:"
    Pop $hCtl_PassBox_Label1  
    ${NSD_CreatePassword} 21.06u 44.31u 252.1u 12.31u ""
    Pop $hCtl_PassBox_pass  
	; disable next button #1 - Next, 2 - Cancel, 3 - Back
	GetDlgItem $1 $HWNDPARENT 3
	EnableWindow $1 0
	nsDialogs::Show
  ${EndIf}  
FunctionEnd
Function EnterPasswordDialogLeave
 ${NSD_GetText} $hCtl_PassBox_pass $0
 ${If} $0 == ${LockPassword}
    StrCpy $PasswordEntered 0
 ${Else}
    MessageBox MB_OK|MB_ICONEXCLAMATION "Вы ввели неверный цифровой ключ"
	Abort
 ${EndIf} 
FunctionEnd
; ---------------------------------------------------------------------