.data
	one			real8	1.0
	zero		real8	0.0

.code

; ZMIENNE
; rcx pixelArray
; rbx pixelArrayMask
; r8 width
; r9 height
; r10 numberThread
; r11 maxThread
; r12 centerX
; r13 centerY
; r14 pixelArrayMask
; r15 pixelArray
; xmm0 force
; xmm1 vignetteRadius
; xmm2 imageRadius

VignetteAsm proc

; Zapisanie rejestrow
push r10
push r11
push r12
push r13
push r14
push r15
push rdi
push rsi
push rax
push rbx
push rcx
push rdx

mov r11, [rsp + 8*20]
mov r10, [rsp + 8*19]
movsd xmm1, qword ptr [rsp + 8*18]
movsd xmm0, qword ptr [rsp + 8*17]	; wczytywanie argumentow


; Obliczenie œrodka obrazu
mov rdx, 0			; int centerX = width / 2;
mov rax, r8
mov rbx, 2
div rbx
mov r12, rax 

mov rdx, 0			; int centerY = height / 2;
mov rax, r9
div rbx
mov r13, rax

; Oblicz imageRadius które odpowiada za ustalenie promienia obrazu potrzebnego do winietowania
cvtsi2sd xmm2, r8	; konwersja na double
cvtsi2sd xmm3, r9
mulsd xmm2, xmm2	; width^2
mulsd xmm3, xmm3	; height^2
addsd xmm2, xmm3	; width^2+height^2
sqrtsd xmm2, xmm2	; imageRadius = sqrt(width^2+height^2)

; Obliczanie wartoœci pêtli która odpowiada za generowanie maski, tak aby zosta³a odpowiednio rozplanowana pomiêdzy w¹tkami
					; Oblicz pocz¹tkow¹ wartoœæ pêtli
mov rax, R8			; rax = width
imul rax, R9		; rax = width * height
cqo					; rozszerz rdx:rax dla dzielenia
idiv R11			; rax = (width * height) / maxThread
imul rax, R10		; rax = numberThread * ((width * height) / maxThread)
mov rsi, rax		; rsi = pocz¹tkowa wartoœæ pêtli

					; Oblicz koñcow¹ wartoœæ pêtli
mov rax, R8			; rax = width
imul rax, R9		; rax = width * height
cqo					; rozszerz rdx:rax dla dzielenia
idiv R11			; rax = (width * height) / maxThread
inc R10				; numberThread + 1
imul rax, R10		; rax = (numberThread + 1) * ((width * height) / maxThread)
dec R10				; Przywróæ R10
mov rdi, rax		; rdi = koñcowa wartoœæ pêtli

pop r14				; pixelArrayMask = r14
pop r15				; pixelArray = r15
push r15
push r14
cvtsi2sd xmm3, r12		; centerX w xmm3
cvtsi2sd xmm4, r13		; centerY w xmm4

; Rozpoczêcie pêtli do obliczania maski
loop_pixelMask_start:

	cmp rsi, rdi
	jge loop_pixelMask_end

	; Operacja dzielenia razem z reszt¹ w celu ustalenia aktualnej pozycji na masce
						; wynik = div(i, width)
	mov rax, rsi		; dzielenie z reszta i
	xor rdx, rdx
	div r8
	cvtsi2sd xmm5, rax	; quot
	cvtsi2sd xmm6, rdx	; rem

	; Obliczanie wartoœci maski
	subsd xmm6, xmm3	; xmm6 = wynik.rem - centerX
	subsd xmm5, xmm4	; xmm5 = wynik.quot - centerY
	mulsd xmm6, xmm6	; xmm6 = (wynik.rem - centerX)^2
	mulsd xmm5, xmm5	; xmm5 = (wynik.quot - centerY)^2
	addsd xmm5, xmm6	; xmm5 = (wynik.rem - centerX)^2 + (wynik.quot - centerY)^2
	sqrtsd xmm5, xmm5	; xmm5 = sqrt((wynik.rem - centerX)^2 + (wynik.quot - centerY)^2)
	divsd xmm5, xmm2	; xmm5 = (sqrt(pow(wynik.rem - centerX, 2) + pow(wynik.quot - centerY, 2))) / imageRadius
	subsd xmm5, xmm1	; xmm5 = ((sqrt(pow(wynik.rem - centerX, 2) + pow(wynik.quot - centerY, 2))) / imageRadius) - vignetteRadius
	mulsd xmm5, xmm0	; xmm5 = force * (((sqrt(pow(wynik.rem - centerX, 2) + pow(wynik.quot - centerY, 2))) / imageRadius) - vignetteRadius)

	; Rozpoczêcie obliczania szeregu Taylora z "x" potrzebnego do obliczeia wartoœci maski
	mov rax, 1
	mov rbx, 2
	movsd xmm6, [one]	; 1 wykonanie Taylora (1)
	addsd xmm6, xmm5	; 2 wykonanie Taylora (1+x)
	movsd xmm7, xmm5	; ustawienie mno¿nika na x

	loop_taylor_start:

		cmp rbx, 18			; wykonuj do osiagniecia 18
		je loop_taylor_end

		mulsd xmm7, xmm5	; kolejna potega x
		mul rbx
		cvtsi2sd xmm8, rax	; kolejna silnia
		movsd xmm9, xmm7
		divsd xmm9, xmm8	; podzielenie potegi przez silnie
		addsd xmm6, xmm9	; dodanie kolejnego wykonania Taylora do zmiennej

		inc rbx				; inkrementacja licznika
		jmp loop_taylor_start

	loop_taylor_end:
	
	; Obsluzenie bledu Taylora poniewa¿ szereg taylora jest wartoœci¹ przybli¿on¹ i istnieje ryzyko wyjœcia poza zakres co skoñczy³o by siê przeskoczeniem wartoœci pixela np. z 255 do 0
	movsd xmm7, [zero]
	ucomisd xmm6, xmm7		; sprawdz czy xmm6 < 0, jesli tak to xmm6 = 0
	jae skip_set_to_zero
	movsd xmm6, xmm7
	skip_set_to_zero:

	; Kontynuacja obliczania maski
	addsd xmm6, [one]	; xmm6 = 1 + pow(2.71828182845904, force * (((sqrt(pow(wynik.rem - centerX, 2) + pow(wynik.quot - centerY, 2))) / imageRadius) - vignetteRadius))
	movsd xmm5, [one]
	divsd xmm5, xmm6	; xmm5 = 1 / (1 + pow(2.71828182845904, force * (((sqrt(pow(wynik.rem - centerX, 2) + pow(wynik.quot - centerY, 2))) / imageRadius) - vignetteRadius)))
	
	movsd qword ptr [r14 + rsi * 8], xmm5

	inc rsi
	jmp loop_pixelMask_start ; kontynuowanie pêtli

loop_pixelMask_end:

; Obliczanie wartoœci pêtli która odpowiada za mno¿enie wartoœci pixeli przez maskê, tak aby zosta³a odpowiednio rozplanowana pomiêdzy w¹tkami
					; Oblicz pocz¹tkow¹ wartoœæ pêtli
mov rax, R8			; rax = width
imul rax, R9		; rax = width * height
imul rax, 3
cqo					; rozszerz rdx:rax dla dzielenia
idiv R11			; rax = (width * height) / maxThread
imul rax, R10		; rax = numberThread * ((width * height) / maxThread)
mov rsi, rax		; rsi = pocz¹tkowa wartoœæ pêtli

					; Oblicz koñcow¹ wartoœæ pêtli
mov rax, R8			; rax = width
imul rax, R9		; rax = width * height
imul rax, 3
cqo					; rozszerz rdx:rax dla dzielenia
idiv R11			; rax = (width * height) / maxThread
inc R10				; numberThread + 1
imul rax, R10		; rax = (numberThread + 1) * ((width * height) / maxThread)
dec R10				; Przywróæ R10
mov rdi, rax		; rdi = koñcowa wartoœæ pêtli

; Rozpoczêcie pêtli odpowiedzialnej za mno¿enie wartoœci pixeli przez maskê
loop_setPixel_start:

	cmp rsi, rdi
	jge loop_setPixel_end

	mov rax, rsi
	mov rcx, 3
	xor rdx, rdx
	div rcx									; ustawienie indeksu/3
	movsd xmm5, qword ptr [r14 + rax * 8]	; odczytanie pixelArrayMask
	mov bl, byte ptr [r15 + rsi]			; odczytanie pixelArray
	movzx rbx, bl							; konwersja na rbx
	cvtsi2sd xmm6, rbx
	mulsd xmm6, xmm5						; pixelArray*=pixelArrayMask
	cvttsd2si rbx, xmm6
	mov byte ptr [r15 + rsi], bl			; zapis pixelArray

	inc rsi									; i++
	jmp loop_setPixel_start

loop_setPixel_end:

; Przywrocenie rejestrow
pop rdx
pop rcx
pop rbx
pop rax
pop rsi
pop rdi
pop r15
pop r14
pop r13
pop r12
pop r11
pop r10

ret
VignetteAsm endp
end