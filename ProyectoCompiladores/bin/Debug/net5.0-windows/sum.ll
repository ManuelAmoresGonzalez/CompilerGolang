; ModuleID = 'AlphaObj'
source_filename = "AlphaObj"
target datalayout = "e-m:w-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128"

@v = global i1 false
@va = global i32 13
@va2 = global i32 72
@val = global double 3.200000e+00
@format = private unnamed_addr constant [24 x i8] c"Este es el rawstringlit\00", align 1

declare i32 @puts(i8*) #0

define i32 @main() #0 {
entry:
  ret i32 0
}

define i32 @entero() #0 {
entry:
  store i1 false, i1* @v, align 1
  store i32 13, i32* @va, align 4
  store double 3.200000e+00, double* @val, align 8
  ret i32 0
}

define i1 @booleana() #0 {
entry:
  %imprimir = call i32 @puts(i8* getelementptr inbounds ([24 x i8], [24 x i8]* @format, i32 0, i32 0))
  ret i1 false
}

define double @flotante() #0 {
entry:
  %variableinterna = alloca i32, align 4
  store i32 6, i32* %variableinterna, align 4
  %otravariable = alloca i1, align 1
  store i1 true, i1* %otravariable, align 1
  ret double 0.000000e+00
}

attributes #0 = { "frame-pointer"="all" }
