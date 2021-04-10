del MindGrind_Builds\*.* /s /f /q
rmdir MindGrind_Builds /s /q

mkdir MindGrind_Builds
mkdir MindGrind_Builds\Linux
mkdir MindGrind_Builds\Windows
mkdir MindGrind_Builds\OSX

cd MindGrind
"C:\Program Files\Unity\Hub\Editor\2021.1.0f1\Editor\Unity.exe" -batchmode -buildWindows64Player ..\MindGrind_Builds\Windows -quit

pause