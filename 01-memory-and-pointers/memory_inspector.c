#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>

/*
 * ============================================================================
 * LABORATÓRIO 01: ANATOMIA DA MEMÓRIA & PONTEIROS (C / LINUX x86_64)
 * ============================================================================
 *
 * Objetivo: Desmistificar a abstração "variável é uma caixinha".
 * Na máquina física, cada variável é uma região de bytes contíguos na RAM.
 */

// Variável estática/global: reside no segmento de dados (.data / .bss)
static int global_var = 100;

void print_separator(const char *title) {
    printf("\n================================================================\n");
    printf(" %s\n", title);
    printf("================================================================\n");
}

int main(void) {
    print_separator("1. VARIÁVEIS NA STACK (PILHA DE EXECUÇÃO)");

    // Variáveis locais alocadas automaticamente na STACK
    int a = 42;
    int b = 84;
    double d = 3.1415926535;
    char c = 'Z';

    printf("Tipo        | Tamanho | Valor        | Endereço na RAM (Hex)\n");
    printf("------------+---------+--------------+----------------------\n");
    printf("int a       | %2zu bytes| %-12d | %p\n", sizeof(a), a, (void *)&a);
    printf("int b       | %2zu bytes| %-12d | %p\n", sizeof(b), b, (void *)&b);
    printf("double d    | %2zu bytes| %-12.6f | %p\n", sizeof(d), d, (void *)&d);
    printf("char c      | %2zu bytes| %-12c | %p\n", sizeof(c), c, (void *)&c);

    // Repara na proximidade dos endereços na Stack (cresce para baixo em x86_64)
    intptr_t diff = (intptr_t)&a - (intptr_t)&b;
    printf("\n--> Distância em bytes entre &a e &b na Stack: %ld bytes\n", (long)diff);

    print_separator("2. PONTEIROS: MANIPULAÇÃO DIRETA DE ENDEREÇOS");

    // Um ponteiro guarda o endereço de outra variável
    int *ptr = &a;

    printf("ptr (guarda o endereço de a)       : %p\n", (void *)ptr);
    printf("Endereço do próprio ponteiro (&ptr): %p\n", (void *)&ptr);
    printf("Valor apontado por ptr (*ptr)      : %d\n", *ptr);

    // Modificação direta na memória através da desreferenciação (*)
    *ptr = 999;
    printf("\n[Após executar '*ptr = 999;']\n");
    printf("Novo valor de a                    : %d (modificado sem tocar no nome 'a'!)\n", a);

    print_separator("3. ARITMÉTICA DE PONTEIROS (SCALING DE TIPOS)");

    int array[4] = {10, 20, 30, 40};
    int *arr_ptr = array;

    printf("Array: [10, 20, 30, 40] contíguo na memória:\n");
    for (int i = 0; i < 4; i++) {
        // arr_ptr + i avança i * sizeof(int) = i * 4 bytes
        printf("  array[%d] -> Valor: %2d | Endereço: %p (deslocamento: +%zu bytes)\n",
               i, *(arr_ptr + i), (void *)(arr_ptr + i), i * sizeof(int));
    }

    print_separator("4. HEAP vs STACK (ALOCAÇÃO DINÂMICA)");

    // malloc requisita blocos de memória ao Sistema Operativo na HEAP
    int *heap_arr = (int *)malloc(3 * sizeof(int));
    if (heap_arr == NULL) {
        perror("Falha na alocação de memória");
        return 1;
    }

    heap_arr[0] = 1000;
    heap_arr[1] = 2000;
    heap_arr[2] = 3000;

    printf("Endereço da variável global (Data Seg): %p\n", (void *)&global_var);
    printf("Endereço do bloco na HEAP (malloc)    : %p\n", (void *)heap_arr);
    printf("Endereço da variável na STACK (&a)    : %p\n", (void *)&a);

    printf("\n--> Observação arquitetural:\n");
    printf("    A HEAP e a STACK vivem em regiões completamente opostas do espaço\n");
    printf("    virtual do processo. A Stack é gerida pelo compilador; a Heap pelo programador.\n");

    // Libertação obrigatória de memória na Heap para evitar Memory Leaks
    free(heap_arr);
    heap_arr = NULL; // Boa prática: evitar dangling pointer

    print_separator("LABORATÓRIO CONCLUÍDO COM SUCESSO");
    return 0;
}
