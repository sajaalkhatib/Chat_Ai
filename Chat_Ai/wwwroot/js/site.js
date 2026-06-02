
    // الشات الحالي
    let currentChatId = null;

    // إرسال رسالة
    async function sendMessage() {
        const userInput = document.getElementById('userInput');
        const message = userInput.value.trim();

        if (!message) return;

        // إضافة رسالة المستخدم للواجهة
        addMessageToUI(message, 'user');
        userInput.value = '';

        // إظهار مؤشر الكتابة
        showTypingIndicator();

        try {
            // إرسال الطلب للـ ChatController
            const response = await fetch('/Chat/SendMessage', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ message: message, chatId: currentChatId })
            });

            if (response.status === 401) {
                hideTypingIndicator();
                addMessageToUI('⚠️ يرجى تسجيل الدخول أولاً للاستخدام.', 'bot');
                return;
            }

            const data = await response.json();

            // إخفاء مؤشر الكتابة
            hideTypingIndicator();

            // إضافة رد البوت للواجهة
            addMessageToUI(data.reply, 'bot');

            // تحديث chatId الحالي
            if (data.chatId) {
                const isNewChat = (currentChatId === null);
                currentChatId = data.chatId;

                // تحديث السايدبار إذا كانت chat جديدة
                if (isNewChat) {
                    loadChatHistory();
                }
            }

        } catch (error) {
            hideTypingIndicator();
            addMessageToUI('Sorry, there was an error. Please try again.', 'bot');
            console.error('Error:', error);
        }
    }

    // إضافة رسالة للواجهة
    function addMessageToUI(text, sender) {
        const chatMessages = document.getElementById('chatMessages');
        const messageDiv = document.createElement('div');
        messageDiv.className = `message ${sender}`;

        const time = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

        if (sender === 'bot') {
            messageDiv.innerHTML = `
                <div class="bot-icon"><i class="bi bi-robot"></i></div>
                <div class="message-content">
                    ${formatMessage(text)}
                    <div class="message-time">${time}</div>
                </div>
            `;
        } else {
            messageDiv.innerHTML = `
                <div class="message-content">
                    ${escapeHtml(text)}
                    <div class="message-time text-end">${time}</div>
                </div>
            `;
        }

        chatMessages.appendChild(messageDiv);
        scrollToBottom();
    }

    // تنسيق رسائل البوت (دعم markdown بسيط)
    function formatMessage(text) {
        let formatted = escapeHtml(text);
        // Bold: **text**
        formatted = formatted.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');
        // Inline code: `code`
        formatted = formatted.replace(/`([^`]+)`/g, '<code>$1</code>');
        // Line breaks
        formatted = formatted.replace(/\n/g, '<br>');
        return formatted;
    }

    // إظهار مؤشر الكتابة
    function showTypingIndicator() {
        const chatMessages = document.getElementById('chatMessages');
        const typingDiv = document.createElement('div');
        typingDiv.className = 'message bot';
        typingDiv.id = 'typingIndicator';
        typingDiv.innerHTML = `
        <div class="bot-icon"><i class="bi bi-robot"></i></div>
        <div class="typing-indicator">
            <span></span>
            <span></span>
            <span></span>
        </div>
        `;
        chatMessages.appendChild(typingDiv);
        scrollToBottom();
    }

    // إخفاء مؤشر الكتابة
    function hideTypingIndicator() {
        const indicator = document.getElementById('typingIndicator');
        if (indicator) {
            indicator.remove();
        }
    }

    // بدء chat جديدة
    function startNewChat() {
        currentChatId = null;
        const chatMessages = document.getElementById('chatMessages');
        chatMessages.innerHTML = `
        <div class="message bot">
            <div class="bot-icon"><i class="bi bi-robot"></i></div>
            <div class="message-content">
                Hello! 👋 I'm your AI assistant. How can I help you today?
                <div class="message-time">Just now</div>
            </div>
        </div>
        `;

        // إزالة active من كل العناصر بالسايدبار
        document.querySelectorAll('.history-item').forEach(el => el.classList.remove('active'));
    }

    // مسح الشات (نفس بدء chat جديدة)
    function clearChat() {
        startNewChat();
    }

    // تحميل هيستوري المحادثات بالسايدبار
    async function loadChatHistory() {
        try {
            const response = await fetch('/Chat/History');
            if (response.status === 401) return; // المستخدم غير مسجل

            const chats = await response.json();
            const historyContainer = document.getElementById('chatHistoryList');
            if (!historyContainer) return;

            historyContainer.innerHTML = '';

            if (chats.length === 0) {
                historyContainer.innerHTML = '<div class="text-muted px-3 py-2" style="font-size: 0.8rem;">No chats yet</div>';
                return;
            }

            chats.forEach(chat => {
                const item = document.createElement('a');
                item.href = '#';
                item.className = 'history-item text-truncate';
                if (chat.id === currentChatId) {
                    item.classList.add('active');
                }
                item.textContent = chat.title || 'New Chat';
                item.dataset.chatId = chat.id;

                item.addEventListener('click', (e) => {
                    e.preventDefault();
                    loadChat(chat.id);
                });

                // زر حذف
                const deleteBtn = document.createElement('button');
                deleteBtn.className = 'delete-chat-btn';
                deleteBtn.innerHTML = '<i class="bi bi-trash3"></i>';
                deleteBtn.title = 'Delete chat';
                deleteBtn.addEventListener('click', (e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    deleteChat(chat.id);
                });

                item.appendChild(deleteBtn);
                historyContainer.appendChild(item);
            });

        } catch (error) {
            console.error('Error loading chat history:', error);
        }
    }

    // تحميل chat معينة
    async function loadChat(chatId) {
        try {
            const response = await fetch(`/Chat/Messages?chatId=${chatId}`);
            if (!response.ok) return;

            const messages = await response.json();
            const chatMessages = document.getElementById('chatMessages');
            chatMessages.innerHTML = '';

            currentChatId = chatId;

            if (messages.length === 0) {
                chatMessages.innerHTML = `
                <div class="message bot">
                    <div class="bot-icon"><i class="bi bi-robot"></i></div>
                    <div class="message-content">
                        Hello! 👋 I'm your AI assistant. How can I help you today?
                        <div class="message-time">Just now</div>
                    </div>
                </div>`;
                return;
            }

            messages.forEach(msg => {
                const sender = msg.senderType === 0 ? 'user' : 'bot';
                const time = new Date(msg.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

                const messageDiv = document.createElement('div');
                messageDiv.className = `message ${sender}`;

                if (sender === 'bot') {
                    messageDiv.innerHTML = `
                        <div class="bot-icon"><i class="bi bi-robot"></i></div>
                        <div class="message-content">
                            ${formatMessage(msg.content)}
                            <div class="message-time">${time}</div>
                        </div>`;
                } else {
                    messageDiv.innerHTML = `
                        <div class="message-content">
                            ${escapeHtml(msg.content)}
                            <div class="message-time text-end">${time}</div>
                        </div>`;
                }

                chatMessages.appendChild(messageDiv);
            });

            scrollToBottom();

            // تحديث active بالسايدبار
            document.querySelectorAll('.history-item').forEach(el => {
                el.classList.toggle('active', parseInt(el.dataset.chatId) === chatId);
            });

        } catch (error) {
            console.error('Error loading chat:', error);
        }
    }

    // حذف chat
    async function deleteChat(chatId) {
        if (!confirm('Are you sure you want to delete this chat?')) return;

        try {
            const response = await fetch(`/Chat/DeleteChat?chatId=${chatId}`, { method: 'POST' });
            if (response.ok) {
                // إذا كنا بنفس الشات، ابدأ واحدة جديدة
                if (currentChatId === chatId) {
                    startNewChat();
                }
                loadChatHistory();
            }
        } catch (error) {
            console.error('Error deleting chat:', error);
        }
    }

    // التمرير للأسفل
    function scrollToBottom() {
        const chatMessages = document.getElementById('chatMessages');
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }

    // معالج Enter
    function handleKeyPress(event) {
        if (event.key === 'Enter') {
            sendMessage();
        }
    }

    // حماية من XSS
    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    // تحميل الهيستوري عند فتح الصفحة
    document.addEventListener('DOMContentLoaded', function () {
        loadChatHistory();
    });
