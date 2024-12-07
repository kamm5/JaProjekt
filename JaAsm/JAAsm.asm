.data
	one   real8  1.0

.code
MyProc1 proc
add RCX, RDX
mov RAX, RCX
ret
MyProc1 endp

; ZMIENNE
; rcx pixelArray
; rbx pixelArrayMask
; r8 width
; r9 height
; r10 numberThread
; r11 maxThread
; r12 centerX
; r13 centerY
; r14
; r15
; xmm0 force
; xmm1 vignetteRadius
; xmm2 imageRadius

VignetteAsm proc
;mov al, byte ptr [rcx+1]

push r10
push r11
push r12
push r13
push r14
push r15
push rax
push rbx
push rcx
push rdx

mov r11, [rsp + 8*18]
mov r10, [rsp + 8*17]
movsd xmm1, qword ptr [rsp + 8*16]
movsd xmm0, qword ptr [rsp + 8*15]	; wczytywanie argumentow


mov rdx, 0			; int centerX = width / 2;
mov rax, r8
mov rbx, 2
div rbx
mov r12, rax 

mov rdx, 0			; int centerY = height / 2;
mov rax, r9
div rbx
mov r13, rax

					; Oblicz imageRadius
cvtsi2sd xmm2, r8	; konwersja na double
cvtsi2sd xmm3, r9
mulsd xmm2, xmm2	; width^2
mulsd xmm3, xmm3	; height^2
addsd xmm2, xmm3	; width^2+height^2
sqrtsd xmm2, xmm2	; imageRadius = sqrt(width^2+height^2)

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

loop_start1:
	cmp rsi, rdi
	jge loop_end1

	; wynik = div(i, width)
	mov rax, rsi		; dzielenie z reszta i
	xor rdx, rdx
	div r8
	cvtsi2sd xmm5, rax	; quot
	cvtsi2sd xmm6, rdx	; rem

	subsd xmm6, xmm3	; xmm6 = wynik.rem - centerX
	subsd xmm5, xmm4	; xmm5 = wynik.quot - centerY
	mulsd xmm6, xmm6	; xmm6 = (wynik.rem - centerX)^2
	mulsd xmm5, xmm5	; xmm5 = (wynik.quot - centerY)^2
	addsd xmm5, xmm6	; xmm5 = (wynik.rem - centerX)^2 + (wynik.quot - centerY)^2
	sqrtsd xmm5, xmm5	; xmm5 = sqrt((wynik.rem - centerX)^2 + (wynik.quot - centerY)^2)
	divsd xmm5, xmm2	; xmm5 = (sqrt(pow(wynik.rem - centerX, 2) + pow(wynik.quot - centerY, 2))) / imageRadius
	subsd xmm5, xmm1	; xmm5 = ((sqrt(pow(wynik.rem - centerX, 2) + pow(wynik.quot - centerY, 2))) / imageRadius) - vignetteRadius
	mulsd xmm5, xmm0	; xmm5 = force * (((sqrt(pow(wynik.rem - centerX, 2) + pow(wynik.quot - centerY, 2))) / imageRadius) - vignetteRadius)

	mov rax, 1
	mov rbx, 2
	movsd xmm6, [one]	; 1 wykonanie Taylora (1)
	addsd xmm6, xmm5	; 2 wykonanie Taylora (1+x)
	movsd xmm7, xmm5	; ustawienie mno¿nika na x

	loop_start2:
		cmp rbx, 10		; wykonuj do osiagniecia 10
		je loop_end2

		mulsd xmm7, xmm5	; kolejna potega x
		mul rbx
		cvtsi2sd xmm8, rax	; kolejna silnia
		movsd xmm9, xmm7
		divsd xmm9, xmm8	; podzielenie potegi przez silnie
		addsd xmm6, xmm9	; dodanie kolejnego wykonania Taylora do zmiennej

		inc rbx				; inkrementacja licznika
		jmp loop_start2
	loop_end2:

	addsd xmm6, [one]		; xmm6 = 1 + pow(2.71828182845904, force * (((sqrt(pow(wynik.rem - centerX, 2) + pow(wynik.quot - centerY, 2))) / imageRadius) - vignetteRadius))
	movsd xmm5, [one]
	divsd xmm5, xmm6		; xmm5 = 1 / (1 + pow(2.71828182845904, force * (((sqrt(pow(wynik.rem - centerX, 2) + pow(wynik.quot - centerY, 2))) / imageRadius) - vignetteRadius)))

	
	movsd qword ptr [r14 + rsi], xmm5

;	movsd qword ptr [r14 + rsi], xmm3

	inc rsi
	jmp loop_start1

loop_end1:

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

loop_start3:
	cmp rsi, rdi
	jge loop_end3

	mov rax, rsi
	mov rcx, 3
	div rcx
	movsd xmm5, qword ptr [r14 + rax]
	movzx rbx, byte ptr [r15 + rsi]
	cvtsi2sd xmm6, rbx
	mulsd xmm6, xmm5
	cvttsd2si rbx, xmm6
	mov byte ptr [r15 + rsi], al

	inc rsi
	jmp loop_start3

loop_end3:

pop rdx
pop rcx
pop rbx
pop rax
pop r15
pop r14
pop r13
pop r12
pop r11
pop r10

ret
VignetteAsm endp
end