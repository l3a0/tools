REM https://www.windows-commandline.com/how-to-create-large-dummy-file/
REM Fsutil.exe is a built in filesystem tool that is useful to do file system related operations from command line.
REM We can create a file of required size using this tool.
REM syntax to create a file:
REM fsutil file createnew filename length
REM fsutil file createnew test.txt 52428800
REM Note that the above command creates a sparse file which does not have any real data. If you want to create a file with real data then you can use the below command line script.
REM echo "This is just a sample line appended to create a big file.. " > dummy.txt
REM for /L %i in (1,1,14) do type dummy.txt >> dummy.txt
REM (Run the above two commands one after another or you can add them to a batch file.)
REM The above commands create a 1 MB file dummy.txt within few seconds.  If you want to create 1 GB file you need to change the second command as below.

REM echo "This is just a sample line appended to create a big file.. " > dummy.txt
REM for /L %i in (1,1,14) do type dummy.txt >> dummy.txt

fsutil file createnew test.txt 10000000