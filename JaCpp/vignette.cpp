#include "vignette.h"
#include "pch.h"
#include <cmath>
#include <cstdlib>

extern "C" __declspec(dllexport)
int Myproc(int a, int b)
{
	return a + b;
}

extern "C" __declspec(dllexport)
void Vignette(unsigned char* pixelArray, double* pixelArrayMask, int width, int height, double force, double vignetteRadius, int numberThread, int maxThread)
{
	int centerX = width / 2;
	int centerY = height / 2;
	div_t wynik;
	double imageRadius = sqrt(pow(width, 2) + pow(height, 2));

	for (int i = numberThread * (width * height) / maxThread; i < (numberThread + 1) * (width * height) / maxThread; i++)
	{
		wynik = div(i, width);
		//pixelArrayMask[i] = 1 - pow((sqrt(pow(wynik.rem - centerX, 2) + pow(wynik.quot - centerY, 2))) / imageRadius, force);
		pixelArrayMask[i] = 1 / (1 + pow(2.71828182845904, force * (((sqrt(pow(wynik.rem - centerX, 2) + pow(wynik.quot - centerY, 2))) / imageRadius) - vignetteRadius)));
	}

	for (int i = numberThread * (width * height * 3) / maxThread; i < (numberThread + 1) * (width * height * 3) / maxThread; ++i)
	{
		pixelArray[i] *= pixelArrayMask[i / 3];
	}
}