#pragma once

extern "C" __declspec(dllexport)
void VignetteCpp(unsigned char* pixelArray, double* pixelArrayMask, int width, int height, double force, double vignetteRadius);