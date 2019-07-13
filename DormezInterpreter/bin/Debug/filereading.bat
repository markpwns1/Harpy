@echo off
setlocal EnableExtensions
setlocal EnableDelayedExpansion

REM -- SETTING WINDOW TITLE
set window_title=Harpy Program
title Harpy Program

REM -- GLOBAL VARIABLE DECLARATION
set /a "var_array_id=0"
set "var_filename=test.txt"

REM -- CALL TO ENTRY POINT
call :func_14 %1 %2 %3 %4 %5 %6 %7 %8 %9
goto :EOF

REM -- BEGIN FUNCTION DECLARATION: file_exists(string): bool
:func_0
set "var_path_F0=%~1"
set "temp_0=exist !var_path_F0!"
if %temp_0% (
goto if_1
)
goto if_1_end
:if_1
set "return_value=1==1"
set "var_path_F0="
goto :EOF
goto if_1_end
:if_1_end
set "return_value=1==0"
goto :EOF

REM -- BEGIN FUNCTION DECLARATION: file_append_line(string, string): void
:func_1
set "var_path_F1=%~1"
set "var_line_F1=%~2"
echo:!var_line_F1!>>"!var_path_F1!"
set "var_path_F1="
set "var_line_F1="
goto :EOF

REM -- BEGIN FUNCTION DECLARATION: file_append(string, string): void
:func_2
set "var_path_F2=%~1"
set "var_content_F2=%~2"
echo | set /p ^="!var_content_F2!">>"!var_path_F2!"
set "var_path_F2="
set "var_content_F2="
goto :EOF

REM -- BEGIN FUNCTION DECLARATION: file_write_all(string, string): void
:func_3
set "var_path_F3=%~1"
set "var_content_F3=%~2"
echo | set /p ^="!var_content_F3!">"!var_path_F3!"
set "var_path_F3="
set "var_content_F3="
goto :EOF

REM -- BEGIN FUNCTION DECLARATION: file_read_lines(string): string
:func_4
set "var_path_F4=%~1"
set /a i=0
for /f "tokens=*" %%a in (!var_path_F4!) do (
set /a i+=1
)
REM -- FUNCTION CALL TO array_create
call :func_5 %i%
REM -- END FUNCTION CALL
set "var_ptr_F4=%return_value%"
set /a i=0
for /f "tokens=*" %%a in (!var_path_F4!) do (
call :process_line %%a
)
set i=
goto end_process_line
:process_line
REM -- FUNCTION CALL TO array_set
call :func_9 !var_ptr_F4! %i% %*
REM -- END FUNCTION CALL
set /a i+=1
goto :eof
:end_process_line
set "return_value=!var_ptr_F4!"
set "var_path_F4="
set "var_ptr_F4="
goto :EOF

REM -- BEGIN FUNCTION DECLARATION: array_create(int): string
:func_5
set /a "var_length_F5=%~1"
set "var_ptr_F5=array_%var_array_id%"
set /a "temp_1=!var_array_id!"
set "var_%var_ptr_F5%=array id=%temp_1%"
set /a "var_%var_ptr_F5%_length=!var_length_F5!"
set /a var_array_id+=1
set "return_value=!var_ptr_F5!"
set "var_length_F5="
set "var_ptr_F5="
goto :EOF

REM -- BEGIN FUNCTION DECLARATION: array_assert_bounds(string, int): void
:func_6
set "var_ptr_F6=%~1"
set /a "var_index_F6=%~2"
set /a "temp_2=!var_index_F6!"
set /a "temp_3=0"
set "temp_4=1==0"
if %temp_2% LSS %temp_3% set temp_4=1==1
set "temp_5=%temp_4%"
set /a "temp_6=!var_index_F6!"
set /a "temp_7=!var_%var_ptr_F6%_length!"
set "temp_8=1==0"
if %temp_6% GEQ %temp_7% set temp_8=1==1
set "temp_9=%temp_8%"
if %temp_5% (
goto if_2
) else (
if %temp_9% (
goto if_2_elseif_1
)
)
goto if_2_end
:if_2
echo ERROR: Index less than zero
pause
exit
goto if_2_end
:if_2_elseif_1
echo ERROR: Index too high
pause
exit
goto if_2_end
:if_2_end
set "var_ptr_F6="
set "var_index_F6="
goto :EOF

REM -- BEGIN FUNCTION DECLARATION: array_assert_exists(string): void
:func_7
set "var_ptr_F7=%~1"
set "temp_10=1==0"
if defined var_!var_ptr_F7! set temp_10=1==1
set "temp_11=not %temp_10%"
if %temp_11% (
goto if_3
)
goto if_3_end
:if_3
echo ERROR: Array does not exist: !var_ptr_F7!
pause
exit
goto if_3_end
:if_3_end
set "var_ptr_F7="
goto :EOF

REM -- BEGIN FUNCTION DECLARATION: array_length(string): int
:func_8
set "var_ptr_F8=%~1"
REM -- FUNCTION CALL TO array_assert_exists
call :func_7 !var_ptr_F8!
REM -- END FUNCTION CALL
set /a "return_value=!var_%var_ptr_F8%_length!"
set "var_ptr_F8="
goto :EOF

REM -- BEGIN FUNCTION DECLARATION: array_set(string, int, indeterminate): void
:func_9
set "var_ptr_F9=%~1"
set /a "var_index_F9=%~2"
set "var_value_F9=%~3"
REM -- FUNCTION CALL TO array_assert_exists
call :func_7 !var_ptr_F9!
REM -- END FUNCTION CALL
REM -- FUNCTION CALL TO array_assert_bounds
call :func_6 !var_ptr_F9! !var_index_F9!
REM -- END FUNCTION CALL
set "var_%var_ptr_F9%_%var_index_F9%=!var_value_F9!"
set "var_ptr_F9="
set "var_index_F9="
set "var_value_F9="
goto :EOF

REM -- BEGIN FUNCTION DECLARATION: array_get(string, int): indeterminate
:func_10
set "var_ptr_F10=%~1"
set /a "var_index_F10=%~2"
REM -- FUNCTION CALL TO array_assert_exists
call :func_7 !var_ptr_F10!
REM -- END FUNCTION CALL
REM -- FUNCTION CALL TO array_assert_bounds
call :func_6 !var_ptr_F10! !var_index_F10!
REM -- END FUNCTION CALL
set "return_value=!var_%var_ptr_F10%_%var_index_F10%!"
set "var_ptr_F10="
set "var_index_F10="
goto :EOF

REM -- BEGIN FUNCTION DECLARATION: array_print(string): void
:func_11
set "var_ptr_F11=%~1"
REM -- FUNCTION CALL TO array_assert_exists
call :func_7 !var_ptr_F11!
REM -- END FUNCTION CALL
REM -- FUNCTION CALL TO array_length
call :func_8 !var_ptr_F11!
REM -- END FUNCTION CALL
set /a "var_i_F11=0"
:loop1
if not !var_i_F11! LSS %return_value% goto loop1_end
set /a "temp_12=!var_i_F11!"
echo:%temp_12%^: !var_%var_ptr_F11%_%var_i_F11%!
set /a var_i_F11+=1
goto loop1
:loop1_end
set "var_i_F11="
set "var_ptr_F11="
goto :EOF

REM -- BEGIN FUNCTION DECLARATION: array_resize(string, int): void
:func_12
set "var_ptr_F12=%~1"
set /a "var_new_size_F12=%~2"
REM -- FUNCTION CALL TO array_length
call :func_8 !var_ptr_F12!
REM -- END FUNCTION CALL
set /a "var_len_F12=%return_value%"
set /a "temp_13=!var_len_F12!"
set /a "temp_14=!var_new_size_F12!"
set "temp_15=1==0"
if %temp_13% GTR %temp_14% set temp_15=1==1
set "temp_16=%temp_15%"
if %temp_16% (
goto if_4
)
goto if_4_end
:if_4
set /a "var_i_F12=!var_new_size_F12!"
:loop2
if not !var_i_F12! LSS !var_len_F12! goto loop2_end
set "var_%var_ptr_F12%_%var_i_F12%="
set /a var_i_F12+=1
goto loop2
:loop2_end
set "var_i_F12="
goto if_4_end
:if_4_end
set /a "var_%var_ptr_F12%_length=!var_new_size_F12!"
set "var_ptr_F12="
set "var_new_size_F12="
set "var_len_F12="
goto :EOF

REM -- BEGIN FUNCTION DECLARATION: array_delete(string): void
:func_13
set "var_ptr_F13=%~1"
REM -- FUNCTION CALL TO array_length
call :func_8 !var_ptr_F13!
REM -- END FUNCTION CALL
set /a "var_i_F13=0"
:loop3
if not !var_i_F13! LSS %return_value% goto loop3_end
set "var_%var_ptr_F13%_%var_i_F13%="
set /a var_i_F13+=1
goto loop3
:loop3_end
set "var_i_F13="
set "var_%var_ptr_F13%="
set "var_%var_ptr_F13%_length="
set "var_ptr_F13="
goto :EOF

REM -- BEGIN FUNCTION DECLARATION: main(): void
:func_14
REM -- FUNCTION CALL TO file_read_lines
call :func_4 !var_filename!
REM -- END FUNCTION CALL
set "var_lines_F14=%return_value%"
REM -- FUNCTION CALL TO array_print
call :func_11 !var_lines_F14!
REM -- END FUNCTION CALL
pause
set "var_lines_F14="
goto :EOF

