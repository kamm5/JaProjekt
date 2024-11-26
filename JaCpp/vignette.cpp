#include "vignette.h"
#include "pch.h"

extern "C" __declspec(dllexport)
int Myproc(int a, int b)
{
	return a + b;
}