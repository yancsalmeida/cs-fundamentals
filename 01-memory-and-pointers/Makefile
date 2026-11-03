CC = gcc
CFLAGS = -Wall -Wextra -O2 -std=c11
TARGET = memory_inspector

all: $(TARGET)

$(TARGET): memory_inspector.c
	$(CC) $(CFLAGS) -o $(TARGET) memory_inspector.c

run: $(TARGET)
	@./$(TARGET)

clean:
	rm -f $(TARGET)

.PHONY: all run clean
