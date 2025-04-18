#include "vignette.h"
#include "pch.h"
#include <cmath>
#include <cstdlib>

extern "C" __declspec(dllexport)
void VignetteCpp(unsigned char* pixelArray, double* pixelArrayMask, int width, int height, double force, double vignetteRadius, int numberThread, int maxThread)
{
	int centerX = width / 2;
	int centerY = height / 2;
	div_t pozycja;
	double imageRadius = sqrt(pow(width, 2) + pow(height, 2));

	for (int i = numberThread * width * height / maxThread; i < (numberThread + 1) * width * height / maxThread; i++)
	{
		pozycja = div(i, width);
		pixelArrayMask[i] = 1 / (1 + exp(force * (((sqrt(pow(pozycja.rem - centerX, 2) + pow(pozycja.quot - centerY, 2))) / imageRadius) - vignetteRadius)));
	}

	for (int i = numberThread * width * height * 3 / maxThread; i < (numberThread + 1) * width * height * 3 / maxThread; i++)
	{
		pixelArray[i] *= pixelArrayMask[i / 3];
	}
}