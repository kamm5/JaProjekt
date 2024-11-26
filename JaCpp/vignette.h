#pragma once

extern "C" __declspec(dllexport)
int Myproc(int a, int b);

extern "C" __declspec(dllexport)
void Vignette(unsigned char* pixelArray, double* pixelArrayMask, int width, int height, double force);