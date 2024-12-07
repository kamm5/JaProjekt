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

;	movsd qword ptr [r14 + rsi], xmm3

	inc rsi
	jmp loop_start1

loop_end1:

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