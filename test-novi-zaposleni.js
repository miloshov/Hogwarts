const axios = require('axios');

const BASE_URL = 'https://localhost:5241';

async function testNoviZaposleni() {
    console.log('🧪 TEST: Provera novih zaposlenih na dashboard-u');
    
    try {
        // 1. Login
        console.log('\n🔐 Login...');
        const loginResponse = await axios.post(`${BASE_URL}/api/auth/login`, {
            userName: 'admin',
            password: 'admin123'
        }, {
            timeout: 10000,
            httpsAgent: httpsAgent  // ✅ DODATO
        });
        
        const token = loginResponse.data.Token;
        console.log('✅ Login uspešan!');
        
        // 2. Dohvati sve zaposlene da vidimo datume zaposlenja
        console.log('\n📋 Dohvatanje svih zaposlenih...');
        const zaposleniResponse = await axios.get(`${BASE_URL}/api/zaposleni`, {
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            timeout: 10000,
            httpsAgent: httpsAgent  // ✅ DODATO
        });
        
        console.log('📊 Lista zaposlenih sa datumima zaposlenja:');
        console.log('=====================================');
        
        const danas = new Date();
        const pre30Dana = new Date();
        pre30Dana.setDate(danas.getDate() - 30);
        
        console.log(`⏰ Danas: ${danas.toISOString().split('T')[0]}`);
        console.log(`⏰ Pre 30 dana: ${pre30Dana.toISOString().split('T')[0]}`);
        console.log('');
        
        let noviZaposleniCount = 0;
        
        // ✅ ISPRAVKA: Proveri da li je response.data niz ili ima svojstvo 'data'
        const zaposleniData = Array.isArray(zaposleniResponse.data) ? zaposleniResponse.data : zaposleniResponse.data.data;
        
        if (!zaposleniData || zaposleniData.length === 0) {
            console.log('❌ Nema zaposlenih u bazi ili pogrešna struktura podataka');
            console.log('Response struktura:', Object.keys(zaposleniResponse.data));
            return;
        }
        
        zaposleniData.forEach((z, index) => {
            const datumZaposlenja = new Date(z.datumZaposlenja);
            const jeNovi = datumZaposlenja >= pre30Dana && z.isActive;
            
            if (jeNovi) noviZaposleniCount++;
            
            console.log(`${index + 1}. ${z.punoIme}`);
            console.log(`   - Datum zaposlenja: ${datumZaposlenja.toISOString().split('T')[0]}`);
            console.log(`   - IsActive: ${z.isActive}`);
            console.log(`   - Je novi (zadnje 30 dana): ${jeNovi ? '✅ DA' : '❌ NE'}`);
            console.log('');
        });
        
        console.log(`🔢 Ukupno novih zaposlenih (manualno): ${noviZaposleniCount}`);
        
        // 3. Pozovi dashboard API da vidimo šta vraća
        console.log('\n📊 Dohvatanje dashboard statistika...');
        const dashboardResponse = await axios.get(`${BASE_URL}/api/dashboard/statistics`, {
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            timeout: 10000,
            httpsAgent: httpsAgent  // ✅ DODATO
        });
        
        console.log('📈 Dashboard response:');
        console.log('=====================================');
        console.log(JSON.stringify(dashboardResponse.data, null, 2));
        
        console.log(`\n🔍 Dashboard novi zaposleni: ${dashboardResponse.data.noviZaposleni}`);
        console.log(`🔍 Manualno izračunati: ${noviZaposleniCount}`);
        
        if (dashboardResponse.data.noviZaposleni === noviZaposleniCount) {
            console.log('✅ POKLAPAJU SE - logika je ispravna!');
        } else {
            console.log('❌ NE POKLAPAJU SE - ima problema sa logikom!');
        }
        
    } catch (error) {
        console.log('\n❌ GREŠKA!');
        
        if (error.response) {
            console.log('Status:', error.response.status);
            console.log('RESPONSE DATA:');
            console.log('=====================================');
            console.log(JSON.stringify(error.response.data, null, 2));
            console.log('=====================================');
        } else if (error.request) {
            console.log('Nema odgovora od servera');
            console.log('Error code:', error.code);
            console.log('Error message:', error.message);
        } else {
            console.log('Greška u konfiguraciji:', error.message);
        }
    }
}

// Pokreni test
testNoviZaposleni();